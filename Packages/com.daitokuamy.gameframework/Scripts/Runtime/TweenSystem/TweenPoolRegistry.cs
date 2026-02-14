using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// TypeごとにObjectPoolを保持するレジストリ（Player内部でのみ使用）
    /// </summary>
    internal sealed class TweenPoolRegistry {
        private readonly Dictionary<Type, IPool> _pools = new();
        
        /// <summary>
        /// レジストリ内部で使用する非ジェネリックPoolインターフェース
        /// </summary>
        private interface IPool {
            /// <summary>
            /// 要素を取得
            /// </summary>
            object Get();

            /// <summary>
            /// 要素を返却
            /// </summary>
            void Release(object element);
        }

        /// <summary>
        /// UnityEngine.Pool.ObjectPoolを型ごとにラップする実装
        /// </summary>
        private sealed class Pool<T> : IPool where T : class, new() {
            /// <summary>
            /// 既定設定でPoolを構築
            /// </summary>
            public Pool() {
                _pool = new ObjectPool<T>(
                    createFunc: static () => new T(),
                    actionOnGet: null,
                    actionOnRelease: null,
                    actionOnDestroy: null,
                    collectionCheck: false,
                    defaultCapacity: 16,
                    maxSize: 1024);
            }

            /// <summary>
            /// 型付きで要素を取得
            /// </summary>
            public T GetTyped() {
                return _pool.Get();
            }

            /// <summary>
            /// 型付きで要素を返却
            /// </summary>
            public void ReleaseTyped(T element) {
                _pool.Release(element);
            }

            /// <inheritdoc/>
            object IPool.Get() {
                return _pool.Get();
            }

            /// <inheritdoc/>
            void IPool.Release(object element) {
                _pool.Release((T)element);
            }

            private readonly ObjectPool<T> _pool;
        }

        /// <summary>
        /// 型Tの要素を取得（未登録の場合は自動登録）
        /// </summary>
        public T Get<T>() where T : class, new() {
            var type = typeof(T);

            if (!_pools.TryGetValue(type, out var pool)) {
                pool = new Pool<T>();
                _pools.Add(type, pool);
            }

            return ((Pool<T>)pool).GetTyped();
        }

        /// <summary>
        /// 要素を返却（型はelement.GetType()で解決）
        /// </summary>
        public void Release(object element) {
            var type = element.GetType();

            if (!_pools.TryGetValue(type, out var pool)) {
                throw new InvalidOperationException($"Pool not registered for type: {type.FullName}");
            }

            pool.Release(element);
        }

        public bool TryReturn(object element) {
            var type = element.GetType();

            if (!_pools.TryGetValue(type, out var pool)) {
                return false;
            }

            pool.Release(element);
            return true;
        }

        /// <summary>
        /// 全Pool登録情報をクリア
        /// </summary>
        public void Clear() {
            _pools.Clear();
        }
    }
}
