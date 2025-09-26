using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

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
        /// 登録解除用Disposable(複数Interface用)
        /// </summary>
        private class ListDisposable : IDisposable {
            public static readonly Disposable Empty = new(null, null);

            private IServiceContainer _container;
            private Type[] _types;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public ListDisposable(IServiceContainer container, Type[] types) {
                _container = container;
                _types = types;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (_container == null || _types == null || _types.Length <= 0) {
                    return;
                }

                foreach (var type in _types) {
                    _container.Remove(type);
                }

                _types = null;
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
        private readonly IServiceResolver _parentResolver;
        private readonly Dictionary<Type, RegisteredServiceInfo> _registeredServiceInfos = new();
        private readonly List<IDisposable> _disposableServices = new();

        private bool _disposed;

        /// <inheritdoc/>
        string IMonitoredServiceContainer.Label => _label;
        /// <inheritdoc/>
        IMonitoredServiceContainer IMonitoredServiceContainer.Parent => (IMonitoredServiceContainer)_parentResolver;
        /// <inheritdoc/>
        bool IServiceContainer.WithParentDispose => _withParentDispose;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parentResolver">親ServiceResolver</param>
        /// <param name="withParentDispose">親のDisposeに合わせてDisposeを自動で行うか</param>
        /// <param name="label">デバッグ表示用ラベル</param>
        public ServiceContainer(IServiceResolver parentResolver = null, bool withParentDispose = true, string label = "") {
            _label = label;

            ServiceMonitor.AddContainer(this);

            _withParentDispose = withParentDispose;
            _parentResolver = parentResolver;
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
        object IServiceResolver.Resolve(Type type) {
            if (_registeredServiceInfos.TryGetValue(type, out var info)) {
                return GetInstance(info);
            }

            return _parentResolver?.Resolve(type);
        }

        /// <inheritdoc/>
        T IServiceResolver.Resolve<T>() {
            return (T)((IServiceResolver)this).Resolve(typeof(T));
        }

        /// <inheritdoc/>
        void IServiceContainer.Remove(Type type) {
            if (!_registeredServiceInfos.Remove(type, out var info)) {
                return;
            }

            if (info.Instance is IDisposable disposable) {
                if (_disposableServices.Remove(disposable)) {
                    disposable.Dispose();
                }
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
        /// タイプ登録(複数Interface対応バージョン)
        /// </summary>
        /// <param name="interfaceTypes">取得に使う型のリスト</param>
        /// <param name="classType">生成するクラス型</param>
        /// <param name="createFunc">生成用関数</param>
        /// <returns>登録解除用のDisposable</returns>
        public IDisposable Register(Type[] interfaceTypes, Type classType, Func<object> createFunc = null) {
            foreach (var interfaceType in interfaceTypes) {
                if (_registeredServiceInfos.TryGetValue(interfaceType, out _)) {
                    throw new Exception($"Already registered type. Type:{interfaceType}");
                }

                if (!interfaceType.IsAssignableFrom(classType)) {
                    throw new Exception($"Not assignable. Type:{interfaceType}, InstanceType:{classType}");
                }
            }

            var info = new RegisteredServiceInfo { Type = classType, CreateFunc = createFunc, };

            foreach (var interfaceType in interfaceTypes) {
                _registeredServiceInfos[interfaceType] = info;
            }

            return new ListDisposable(this, interfaceTypes);
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

            info = new RegisteredServiceInfo { Type = classType, CreateFunc = createFunc, };

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

            info = new RegisteredServiceInfo { Type = classType, CreateFunc = createFunc, };

            _registeredServiceInfos[classType] = info;
            return new Disposable(this, classType);
        }

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        public IDisposable Register<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T>(Func<T> createFunc = null) {
            return Register(new[] { typeof(TInterface1), typeof(TInterface2), typeof(TInterface3), typeof(TInterface4), typeof(TInterface5) }, typeof(T), createFunc != null ? () => createFunc.Invoke() : null);
        }

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        public IDisposable Register<TInterface1, TInterface2, TInterface3, TInterface4, T>(Func<T> createFunc = null) {
            return Register(new[] { typeof(TInterface1), typeof(TInterface2), typeof(TInterface3), typeof(TInterface4) }, typeof(T), createFunc != null ? () => createFunc.Invoke() : null);
        }

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        public IDisposable Register<TInterface1, TInterface2, TInterface3, T>(Func<T> createFunc = null) {
            return Register(new[] { typeof(TInterface1), typeof(TInterface2), typeof(TInterface3) }, typeof(T), createFunc != null ? () => createFunc.Invoke() : null);
        }

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        public IDisposable Register<TInterface1, TInterface2, T>(Func<T> createFunc = null) {
            return Register(new[] { typeof(TInterface1), typeof(TInterface2) }, typeof(T), createFunc != null ? () => createFunc.Invoke() : null);
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

            info = new RegisteredServiceInfo { Type = type, CreateFunc = null, Instance = instance, };

            _registeredServiceInfos[type] = info;

            Inject(instance);

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
                if (_disposableServices.Remove(disposable)) {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// インスタンスにServiceを注入
        /// </summary>
        public void Inject(object instance) {
            var type = instance.GetType();

            // [ServiceInject]が付いたインスタンスフィールドを収集
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fieldInfos = type.GetFields(flags)
                .Where(f => f.GetCustomAttribute<ServiceInjectAttribute>() != null);

            // フィールドを初期化
            foreach (var fieldInfo in fieldInfos) {
                if (fieldInfo.IsInitOnly) {
                    throw new InvalidOperationException($"{type.FullName}.{fieldInfo.Name} is readonly.");
                }

                if (fieldInfo.FieldType == typeof(IServiceResolver)) {
                    fieldInfo.SetValue(instance, this);
                    continue;
                }

                var service = GetService(fieldInfo.FieldType);
                if (service == null) {
                    throw new InvalidOperationException($"{type.FullName}.{fieldInfo.Name}({fieldInfo.FieldType.Name}) is not found service.");
                }

                fieldInfo.SetValue(instance, service);
            }

            // メソッドを初期化
            var methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<ServiceInjectAttribute>() != null);

            foreach (var methodInfo in methodInfos) {
                var args = GetServiceParameters(methodInfo);
                methodInfo.Invoke(instance, args);
            }
        }

        /// <summary>
        /// コンテナ内のクリア
        /// </summary>
        private void ClearInternal() {
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
        /// Serviceの取得
        /// </summary>
        private object GetService(Type serviceType) {
            if (serviceType == typeof(IServiceResolver)) {
                return this;
            }

            if (!_registeredServiceInfos.TryGetValue(serviceType, out var serviceInfo)) {
                if (_parentResolver is ServiceContainer container) {
                    return container.GetService(serviceType);
                }

                return null;
            }

            return GetInstance(serviceInfo);
        }

        /// <summary>
        /// Method引数用のServiceリストを取得
        /// </summary>
        private object[] GetServiceParameters(MethodBase methodBase) {
            var parameterInfos = methodBase.GetParameters();
            var parameters = new object[parameterInfos.Length];
            for (var i = 0; i < parameterInfos.Length; i++) {
                var s = GetService(parameterInfos[i].ParameterType);
                parameters[i] = s ?? throw new InvalidOperationException($"{methodBase.Name}({parameterInfos[i].ParameterType.Name}) is not found service.");
            }

            return parameters;
        }

        /// <summary>
        /// Inject対応のインスタンス生成
        /// </summary>
        private object CreateInstance(Type type) {
            // コンストラクタ解決
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var markedConstructor = constructors.FirstOrDefault(c => c.GetCustomAttribute<ServiceInjectAttribute>() != null);
            var constructor = default(ConstructorInfo);
            var constructorArgs = default(object[]);

            if (markedConstructor != null) {
                constructor = markedConstructor;
                constructorArgs = GetServiceParameters(markedConstructor);
            }
            else {
                constructor = constructors.FirstOrDefault(x => x.GetParameters().Length == 0);

                if (constructor == null) {
                    // デフォルト引数のみのコンストラクタがあった場合はそれを呼ぶ
                    var argumentList = new List<object>();
                    foreach (var c in constructors) {
                        argumentList.Clear();
                        var parameters = c.GetParameters();
                        foreach (var p in parameters) {
                            if (!p.HasDefaultValue) {
                                constructor = null;
                                break;
                            }

                            constructor = c;
                            argumentList.Add(p.DefaultValue);
                        }

                        if (constructor != null) {
                            break;
                        }
                    }

                    if (constructor == null) {
                        throw new InvalidOperationException($"Not found default constructor: {type.FullName}");
                    }

                    constructorArgs = argumentList.ToArray();
                }
                else {
                    constructorArgs = Array.Empty<object>();
                }
            }

            // コンストラクタ呼び出し
            return constructor.Invoke(constructorArgs);
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

            // インスタンス生成
            if (info.CreateFunc != null) {
                info.Instance = info.CreateFunc();
            }
            else {
                info.Instance = CreateInstance(info.Type);
            }

            // サービスのInject
            Inject(info.Instance);

            // Disposables登録
            if (info.Instance is IDisposable disposable) {
                _disposableServices.Add(disposable);
            }

            return info.Instance;
        }
    }
}