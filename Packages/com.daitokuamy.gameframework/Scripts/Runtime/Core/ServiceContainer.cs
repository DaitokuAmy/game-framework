using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のコンテナ
    /// </summary>
    public class ServiceContainer : IServiceContainer, IMonitoredServiceContainer {
        /// <summary>
        /// 登録解除用Disposable
        /// </summary>
        private class Disposable : IDisposable {
            public static readonly Disposable Empty = new(null, null);

            private IServiceContainer _container;
            private Type _type;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Disposable(IServiceContainer container, Type type) {
                _container = container;
                _type = type;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (_container == null || _type == null) {
                    return;
                }

                _container.Remove(_type);
                _type = null;
                _container = null;
            }
        }

        /// <summary>
        /// 登録されたサービスの情報
        /// </summary>
        private class RegisteredServiceInfo {
            public Type Type;
            public object Instance;
            public Func<object> CreateFunc;
        }

        private readonly string _label;
        private readonly bool _withParentDispose;
        private readonly List<IServiceContainer> _children = new();
        private readonly Dictionary<Type, RegisteredServiceInfo> _registeredServiceInfos = new();
        private readonly List<IDisposable> _disposableServices = new();

        private bool _disposed;

        /// <inheritdoc/>
        string IMonitoredServiceContainer.Label => _label;
        /// <inheritdoc/>
        IReadOnlyList<IMonitoredServiceContainer> IMonitoredServiceContainer.Children => _children.OfType<IMonitoredServiceContainer>().ToArray();
        /// <inheritdoc/>
        bool IServiceContainer.WithParentDispose => _withParentDispose;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親ServiceContainer</param>
        /// <param name="withParentDispose">親のDisposeに合わせてDisposeを自動で行うか</param>
        /// <param name="label">デバッグ表示用ラベル</param>
        public ServiceContainer(IServiceContainer parent = null, bool withParentDispose = true, string label = "") {
            _label = label;

            if (parent == null && GetType() != typeof(Services)) {
                parent = Services.Instance;
            }

            ServiceMonitor.AddContainer(this);

            _withParentDispose = withParentDispose;

            if (parent is ServiceContainer container) {
                container._children.Add(this);
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            ServiceMonitor.RemoveContainer(this);
            ClearInternal();
        }

        /// <inheritdoc/>
        void IMonitoredServiceContainer.GetRegisteredServiceInfos(List<(Type type, object instance)> list) {
            foreach (var pair in _registeredServiceInfos) {
                list.Add((pair.Key, pair.Value.Instance));
            }
        }

        /// <inheritdoc/>
        void IServiceContainer.Clear() {
            ClearInternal();
        }

        /// <inheritdoc/>
        object IServiceContainer.Resolve(Type type) {
            for (var i = _children.Count - 1; i >= 0; i--) {
                var result = _children[i].Resolve(type);
                if (result != null) {
                    return result;
                }
            }

            if (_registeredServiceInfos.TryGetValue(type, out var info)) {
                return GetInstance(info);
            }

            return null;
        }

        /// <inheritdoc/>
        T IServiceContainer.Resolve<T>() {
            return (T)((IServiceContainer)this).Resolve(typeof(T));
        }

        /// <inheritdoc/>
        void IServiceContainer.Remove(Type type) {
            if (!_registeredServiceInfos.Remove(type, out var info)) {
                return;
            }

            if (info.Instance is IDisposable disposable) {
                disposable.Dispose();
            }

            info.Instance = null;
        }

        /// <summary>
        /// サービスの削除
        /// </summary>
        void IServiceContainer.Remove<T>() {
            ((IServiceContainer)this).Remove(typeof(T));
        }

        /// <summary>
        /// タイプ登録
        /// </summary>
        /// <param name="interfaceType">取得に使う型</param>
        /// <param name="classType">生成するクラス型</param>
        /// <param name="createFunc">生成用関数</param>
        /// <returns>登録解除用のDisposable</returns>
        public IDisposable Register(Type interfaceType, Type classType, Func<object> createFunc = null) {
            if (_registeredServiceInfos.TryGetValue(interfaceType, out var info)) {
                throw new Exception($"Already registered type. Type:{interfaceType}");
            }

            if (!interfaceType.IsAssignableFrom(classType)) {
                throw new Exception($"Not assignable. Type:{interfaceType}, InstanceType:{classType}");
            }

            info = new RegisteredServiceInfo {
                Type = classType,
                CreateFunc = createFunc,
            };

            _registeredServiceInfos[interfaceType] = info;
            return new Disposable(this, interfaceType);
        }

        /// <summary>
        /// タイプ登録
        /// </summary>
        /// <param name="classType">生成するクラス型</param>
        /// <param name="createFunc">生成用関数</param>
        /// <returns>登録解除用のDisposable</returns>
        public IDisposable Register(Type classType, Func<object> createFunc = null) {
            if (_registeredServiceInfos.TryGetValue(classType, out var info)) {
                throw new Exception($"Already registered type. Type:{classType}");
            }

            info = new RegisteredServiceInfo {
                Type = classType,
                CreateFunc = createFunc,
            };

            _registeredServiceInfos[classType] = info;
            return new Disposable(this, classType);
        }

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        public IDisposable Register<TInterface, T>(Func<T> createFunc = null) {
            return Register(typeof(TInterface), typeof(T), createFunc != null ? () => createFunc.Invoke() : null);
        }

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        public IDisposable Register<T>(Func<T> createFunc = null) {
            return Register(typeof(T), createFunc != null ? () => createFunc.Invoke() : null);
        }

        /// <summary>
        /// インスタンス登録
        /// </summary>
        /// <param name="type">登録するタイプ</param>
        /// <param name="instance">登録するインスタンス</param>
        /// <returns>登録解除用のDisposable</returns>
        public IDisposable RegisterInstance(Type type, object instance) {
            if (instance == null) {
                return Disposable.Empty;
            }

            if (!type.IsAssignableFrom(instance.GetType())) {
                throw new Exception($"Not assignable. Type:{type}, InstanceType:{instance.GetType()}");
            }

            if (_registeredServiceInfos.TryGetValue(type, out var info)) {
                throw new Exception($"Already registered type. Type:{type}");
            }

            info = new RegisteredServiceInfo {
                Type = type,
                CreateFunc = null,
                Instance = instance,
            };

            _registeredServiceInfos[type] = info;

            if (instance is IDisposable disposable) {
                _disposableServices.Add(disposable);
            }

            return new Disposable(this, type);
        }

        /// <summary>
        /// インスタンス登録
        /// </summary>
        /// <param name="instance">登録するインスタンス</param>
        /// <returns>登録解除用のDisposable</returns>
        public IDisposable RegisterInstance(object instance) {
            return RegisterInstance(instance.GetType(), instance);
        }

        /// <summary>
        /// インスタンス登録
        /// </summary>
        /// <param name="instance">登録するインスタンス</param>
        /// <returns>登録解除用のDisposable</returns>
        public IDisposable RegisterInstance<T>(object instance) {
            return RegisterInstance(typeof(T), instance);
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="type">登録時の型</param>
        public void Remove(Type type) {
            if (!_registeredServiceInfos.Remove(type, out var info)) {
                return;
            }

            if (info.Instance is IDisposable disposable) {
                disposable.Dispose();
                _disposableServices.Remove(disposable);
            }
        }

        /// <summary>
        /// コンテナ内のクリア
        /// </summary>
        private void ClearInternal() {
            // 子を解放
            for (var i = _children.Count - 1; i >= 0; i--) {
                if (!_children[i].WithParentDispose) {
                    continue;
                }

                _children[i].Dispose();
            }

            _children.Clear();

            // 逆順に解放
            for (var i = _disposableServices.Count - 1; i >= 0; i--) {
                var disposable = _disposableServices[i];
                if (disposable == null) {
                    continue;
                }

                disposable.Dispose();
            }

            // サービス参照をクリア
            _disposableServices.Clear();
            _registeredServiceInfos.Clear();
        }

        /// <summary>
        /// 登録したサービス情報からインスタンスを取得
        /// </summary>
        private object GetInstance(RegisteredServiceInfo info) {
            if (info == null) {
                return null;
            }

            if (info.Instance != null) {
                return info.Instance;
            }

            if (info.CreateFunc != null) {
                info.Instance = info.CreateFunc();
            }
            else {
                info.Instance = Activator.CreateInstance(info.Type);
            }

            if (info.Instance is IDisposable disposable) {
                _disposableServices.Add(disposable);
            }

            return info.Instance;
        }
    }
}