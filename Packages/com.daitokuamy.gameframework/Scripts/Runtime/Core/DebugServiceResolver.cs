using System;

namespace GameFramework.Core {
    /// <summary>
    /// Service提供用のインターフェース
    /// </summary>
    public abstract class DebugServiceResolver<TResolver> : IDisposable, IServiceResolver
        where TResolver : DebugServiceResolver<TResolver> {
        private IServiceResolver _resolver;
        
        /// <summary>SingletonInstance</summary>
        public static TResolver Instance { get; set; }

        /// <summary>
        /// サービスのDI
        /// </summary>
        [ServiceInject]
        private void Inject(IServiceResolver resolver) {
            if (Instance != null) {
                throw new InvalidOperationException($"The service resolver has already been imported. [{nameof(TResolver)}]");
            }
            
            _resolver = resolver;
            Instance = (TResolver)this;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        void IDisposable.Dispose() {
            if (Instance == this) {
                Instance = null;
            }
        }

        /// <summary>
        /// インスタンスの取得
        /// </summary>
        public object Resolve(Type type) {
            return _resolver.Resolve(type);
        }

        /// <summary>
        /// インスタンスの取得
        /// </summary>
        public T Resolve<T>() {
            return _resolver.Resolve<T>();
        }

        /// <inheritdoc/>
        public void Inject(object instance) {
            _resolver.Inject(instance);
        }
    }
}