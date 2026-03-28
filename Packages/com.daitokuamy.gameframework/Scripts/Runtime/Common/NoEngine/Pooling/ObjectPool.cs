using System;

namespace GameFramework.Pooling {
    /// <summary>
    /// Unity非依存のPoolクラス
    /// </summary>
    public sealed class ObjectPool<T> : ObjectPool<T, NoContext> 
        where T : class {
        /// <summary>
        /// Pool関数アダプター
        /// </summary>
        private static class PoolAdapters {
            public static Func<NoContext, T> Create(Func<T> f) => _ => f();
            public static Action<NoContext, T> Action(Action<T> a) => a is null ? null : (_, e) => a(e);
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="createFunc">要素生成処理</param>
        /// <param name="actionOnGet">要素取得時処理</param>
        /// <param name="actionOnRelease">要素返却時処理</param>
        /// <param name="actionOnDestroy">要素廃棄時処理</param>
        /// <param name="collectionCheck">不正な返却などがあった際に例外をスローするか</param>
        /// <param name="defaultCapacity">初期状態の要素確保数</param>
        /// <param name="maxSize">要素確保最大数</param>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = false,
            int defaultCapacity = 0,
            int maxSize = int.MaxValue
        )
            : base(
                context: default,
                createFunc: PoolAdapters.Create(createFunc),
                actionOnGet: PoolAdapters.Action(actionOnGet),
                actionOnRelease: PoolAdapters.Action(actionOnRelease),
                actionOnDestroy: PoolAdapters.Action(actionOnDestroy),
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            ) {
        }
    }

    /// <summary>
    /// Unity非依存のPoolクラス
    /// </summary>
    public class ObjectPool<T, TContext> : IDisposable
        where T : class {
        private readonly TContext _context;
        private readonly Func<TContext, T> _createFunc;
        private readonly Action<TContext, T> _actionOnGet;
        private readonly Action<TContext, T> _actionOnRelease;
        private readonly Action<TContext, T> _actionOnDestroy;

        private readonly bool _collectionCheck;
        private readonly int _maxSize;

        private bool _disposed;
        private T[] _items;
        private int _inactiveCount;
        private bool _poolingEnabled = true;

        /// <summary>プールに含まれる要素数</summary>
        public int InactiveCount => _inactiveCount;
        /// <summary>プーリングが有効か</summary>
        public bool PoolingEnabled => _poolingEnabled;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">各種関数コール時渡すコンテキスト情報</param>
        /// <param name="createFunc">要素生成処理</param>
        /// <param name="actionOnGet">要素取得時処理</param>
        /// <param name="actionOnRelease">要素返却時処理</param>
        /// <param name="actionOnDestroy">要素廃棄時処理</param>
        /// <param name="collectionCheck">不正な返却などがあった際に例外をスローするか</param>
        /// <param name="defaultCapacity">初期状態の要素確保数</param>
        /// <param name="maxSize">要素確保最大数</param>
        public ObjectPool(
            TContext context,
            Func<TContext, T> createFunc,
            Action<TContext, T> actionOnGet = null,
            Action<TContext, T> actionOnRelease = null,
            Action<TContext, T> actionOnDestroy = null,
            bool collectionCheck = false,
            int defaultCapacity = 0,
            int maxSize = int.MaxValue
        ) {
            if (defaultCapacity < 0) {
                throw new ArgumentOutOfRangeException(nameof(defaultCapacity));
            }

            if (maxSize < 1) {
                throw new ArgumentOutOfRangeException(nameof(maxSize));
            }

            _context = context;
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _collectionCheck = collectionCheck;
            _maxSize = maxSize;

            _items = defaultCapacity == 0 ? Array.Empty<T>() : new T[defaultCapacity];
            _inactiveCount = 0;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            Clear();
        }

        /// <summary>
        /// プールから要素を取得します。
        /// </summary>
        public T Get() {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(ObjectPool<T>));
            }
            
            // Poolを通さないケース
            if (!_poolingEnabled) {
                var c = _createFunc(_context);
                _actionOnGet?.Invoke(_context, c);
                return c;
            }
            
            // 通常時
            if (_inactiveCount > 0) {
                _inactiveCount--;
                var item = _items[_inactiveCount];
                _items[_inactiveCount] = null;

                var result = item ?? _createFunc(_context);
                _actionOnGet?.Invoke(_context, result);
                return result;
            }

            var created = _createFunc(_context);
            _actionOnGet?.Invoke(_context, created);
            return created;
        }

        /// <summary>
        /// 要素をプールに返却
        /// </summary>
        public void Release(T element) {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(ObjectPool<T>));
            }
            
            if (element == null) {
                throw new ArgumentNullException(nameof(element));
            }
            
            // プールを通さない時
            if (!_poolingEnabled) {
                _actionOnRelease?.Invoke(_context, element);
                _actionOnDestroy?.Invoke(_context, element);
                return;
            }

            // 通常時
            if (_collectionCheck) {
                for (var i = 0; i < _inactiveCount; i++) {
                    if (ReferenceEquals(_items[i], element)) {
                        throw new InvalidOperationException("Trying to release an object that is already in the pool.");
                    }
                }
            }

            if (element is IPoolable poolable) {
                poolable.ResetForPool();
            }

            _actionOnRelease?.Invoke(_context, element);

            if (_inactiveCount >= _maxSize) {
                _actionOnDestroy?.Invoke(_context, element);
                return;
            }

            EnsureCapacity(_inactiveCount + 1);
            _items[_inactiveCount++] = element;
        }

        /// <summary>
        /// 指定数だけ事前生成
        /// </summary>
        public void Prewarm(int count) {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(ObjectPool<T>));
            }
            
            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (!_poolingEnabled) {
                return;
            }

            EnsureCapacity(_inactiveCount + count);

            for (var i = 0; i < count; i++) {
                _items[_inactiveCount++] = _createFunc(_context);
            }
        }

        /// <summary>
        /// プール内の要素を全て破棄
        /// </summary>
        public void Clear() {
            if (_actionOnDestroy is not null) {
                for (var i = 0; i < _inactiveCount; i++) {
                    var item = _items[i];
                    if (item is not null) {
                        _actionOnDestroy(_context, item);
                    }
                }
            }

            Array.Clear(_items, 0, _inactiveCount);
            _inactiveCount = 0;
        }

        /// <summary>
        /// プーリングの有効/無効を切り替え
        /// </summary>
        public void SetPoolingEnabled(bool enabled, bool clearInactive = false) {
            if (_poolingEnabled == enabled) {
                return;
            }

            _poolingEnabled = enabled;

            if (!enabled && clearInactive) {
                Clear();
            }
        }

        /// <summary>
        /// 要素の再確保
        /// </summary>
        private void EnsureCapacity(int required) {
            if (required <= _items.Length) {
                return;
            }

            var newSize = _items.Length == 0 ? 4 : _items.Length * 2;
            if (newSize < required) {
                newSize = required;
            }

            if (newSize > _maxSize) {
                newSize = _maxSize;
            }

            if (newSize <= _items.Length) {
                return;
            }

            var newArray = new T[newSize];
            if (_inactiveCount > 0) {
                Array.Copy(_items, newArray, _inactiveCount);
            }

            _items = newArray;
        }
    }
}