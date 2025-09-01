using System;

namespace GameFramework.Core {
    /// <summary>
    /// Service提供用のインターフェース
    /// </summary>
    public abstract class DebugServiceResolver<TResolver> : IDisposable, IServiceUser, IServiceResolver
        where TResolver : DebugServiceResolver<TResolver> {
        private IServiceResolver _resolver;
        
        /// <summary>SingletonInstance</summary>
        public static TResolver Instance { get; set; }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        void IDisposable.Dispose() {
            if (Instance == this) {
                Instance = null;
            }
        }

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver resolver) {
            if (Instance != null) {
                throw new InvalidOperationException($"The service resolver has already been imported. [{nameof(TResolver)}]");
            }
            
            _resolver = resolver;
            Instance = (TResolver)this;
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
        public void Import(IServiceUser user) {
            if (user == null) {
                return;
            }
            
            user.ImportService(this);
        }
    }
}