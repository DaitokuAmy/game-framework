using System;

namespace GameFramework.Pooling {
    /// <summary>
    /// シンプルなインスタンスPool
    /// </summary>
    public sealed class InstancePool<T> : ObjectPool<T, NoContext>
        where T : class, new() {
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
        /// <param name="actionOnGet">要素取得時処理</param>
        /// <param name="actionOnRelease">要素返却時処理</param>
        /// <param name="collectionCheck">不正な返却などがあった際に例外をスローするか</param>
        /// <param name="defaultCapacity">初期状態の要素確保数</param>
        /// <param name="maxSize">要素確保最大数</param>
        public InstancePool(
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            bool collectionCheck = false,
            int defaultCapacity = 0,
            int maxSize = int.MaxValue
        )
            : base(
                context: default,
                createFunc: PoolAdapters.Create(() => new T()),
                actionOnGet: PoolAdapters.Action(actionOnGet),
                actionOnRelease: PoolAdapters.Action(actionOnRelease),
                actionOnDestroy: PoolAdapters.Action(inst => {
                    if (inst is IDisposable disposable) {
                        disposable.Dispose();
                    }
                }),
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            ) {
        }
    }
}