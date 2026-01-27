using System;
using System.Collections.Generic;

namespace GameFramework.Core {

    /// <summary>
    /// Key管理機能付きObjectPool
    /// </summary>
    public sealed class KeyedObjectPool<TKey, T>
        where TKey : notnull
        where T : class {

        private readonly Dictionary<TKey, ObjectPool<T, TKey>> _pools;
        private readonly PoolFactory _factory;

        /// <summary>ObjectPool返却用の関数定義</summary>
        public delegate ObjectPool<T, TKey> PoolFactory(TKey key);

        /// <summary>内部で保持されているPoolの数</summary>
        public int PoolCount => _pools.Count;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="factory">Pool生成用関数</param>
        /// <param name="initialKeyCapacity">内部確保するDictionaryの初期キャパシティ</param>
        /// <param name="comparer">内部確保するDictionaryのKey比較する処理</param>
        public KeyedObjectPool(
            PoolFactory factory,
            int initialKeyCapacity = 0,
            IEqualityComparer<TKey> comparer = null
        ) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            _pools = initialKeyCapacity == 0
                ? new Dictionary<TKey, ObjectPool<T, TKey>>(comparer)
                : new Dictionary<TKey, ObjectPool<T, TKey>>(initialKeyCapacity, comparer);
        }

        /// <summary>
        /// ObjectPoolの取得
        /// </summary>
        public ObjectPool<T, TKey> GetPool(TKey key) {
            if (_pools.TryGetValue(key, out var pool)) {
                return pool;
            }

            var created = _factory(key);
            _pools.Add(key, created);
            return created;
        }

        /// <summary>
        /// ObjectPoolから要素を取得
        /// </summary>
        public T Get(TKey key) {
            return GetPool(key).Get();
        }

        /// <summary>
        /// ObjectPoolに要素を返却
        /// </summary>
        public void Release(TKey key, T element) {
            GetPool(key).Release(element);
        }

        /// <summary>
        /// ObjectPoolの要素確保数を設定
        /// </summary>
        public void Prewarm(TKey key, int count) {
            GetPool(key).Prewarm(count);
        }

        /// <summary>
        /// ObjectPoolの中身をクリア
        /// </summary>
        public void Clear(TKey key) {
            if (_pools.TryGetValue(key, out var pool)) {
                pool.Clear();
            }
        }

        /// <summary>
        /// 全ObjectPoolの中身をクリア
        /// </summary>
        public void ClearAll() {
            foreach (var pool in _pools.Values) {
                pool.Clear();
            }
        }

        /// <summary>
        /// 全ObjectPoolのPoolingの有効無効切り替え
        /// </summary>
        public void SetPoolingEnabled(bool enabled, bool clearInactive = false) {
            foreach (var pool in _pools.Values) {
                pool.SetPoolingEnabled(enabled, clearInactive);
            }
        }
    }
}