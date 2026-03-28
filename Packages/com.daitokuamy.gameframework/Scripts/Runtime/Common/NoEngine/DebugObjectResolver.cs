#if USE_VCONTAINER
using System;
using VContainer;
using VContainer.Diagnostics;

namespace GameFramework {
    /// <summary>
    /// VContainerのインスタンスをシングルトン的に提供するための基底クラス
    /// </summary>
    public abstract class DebugObjectResolver<TResolver> : IDisposable, IObjectResolver
        where TResolver : DebugObjectResolver<TResolver> {
        private IObjectResolver _resolver;

        /// <summary>SingletonInstance</summary>
        public static TResolver Instance { get; private set; }

        /// <inheritdoc/>
        object IObjectResolver.ApplicationOrigin => _resolver.ApplicationOrigin;
        /// <inheritdoc/>
        DiagnosticsCollector IObjectResolver.Diagnostics {
            get => _resolver.Diagnostics;
            set => _resolver.Diagnostics = value;
        }

        /// <summary>
        /// インスタンス注入
        /// </summary>
        [Inject]
        protected void Construct(IObjectResolver resolver) {
            if (Instance != null) {
                throw new InvalidOperationException($"The service resolver has already been imported. [{nameof(TResolver)}]");
            }

            _resolver = resolver;
            Instance = (TResolver)this;
        }

        /// <inheritdoc/>
        void IDisposable.Dispose() {
            if (Instance == this) {
                Instance = null;
            }
        }

        /// <inheritdoc/>
        object IObjectResolver.Resolve(Type type, object key) {
            return _resolver.Resolve(type, key);
        }

        /// <inheritdoc/>
        bool IObjectResolver.TryResolve(Type type, out object resolved, object key) {
            return _resolver.TryResolve(type, out resolved, key);
        }

        /// <inheritdoc/>
        object IObjectResolver.Resolve(Registration registration) {
            return _resolver.Resolve(registration);
        }

        /// <inheritdoc/>
        IScopedObjectResolver IObjectResolver.CreateScope(Action<IContainerBuilder> installation) {
            return _resolver.CreateScope(installation);
        }

        /// <inheritdoc/>
        void IObjectResolver.Inject(object instance) {
            _resolver.Inject(instance);
        }

        /// <inheritdoc/>
        bool IObjectResolver.TryGetRegistration(Type type, out Registration registration, object key) {
            return _resolver.TryGetRegistration(type, out registration, key);
        }
    }
}
#endif
