using System;
using System.Collections;
using GameFramework;
using GameFramework.AssetSystem;
using GameFramework.BootSystem;
using GameFramework;
using GameFramework.EnvironmentSystem;
using GameFramework.NavigationSystem;
using GameFramework.UISystem;
using SampleGame.Application;
using SampleGame.Infrastructure;
using SampleGame.Presentation;
using ThirdPersonEngine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// アプリケーションのメインシステム
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public partial class MainSystem : MainSystemBase {
        /// <summary>
        /// スタート時の引数
        /// </summary>
        public struct StartArgs {
            public int NodeId;
            public Action<INavNode> SetupAction;
        }

        /// <summary>
        /// リブート時の引数
        /// </summary>
        public struct RebootArgs {
            public int NodeId;
            public Action<INavNode> SetupAction;
        }

        private DisposableScope _globalScope;
        private IObjectResolver _globalResolver;
        private UpdateScheduler _updateScheduler;

        /// <inheritdoc/>
        protected override void PreStartInternal(object[] args) {
            _globalScope = new DisposableScope();

            var builder = new ContainerBuilder();

            _updateScheduler = new UpdateScheduler().RegisterTo(_globalScope);
            builder.RegisterInstance(_updateScheduler);
            UpdatableUtility.Initialize(_updateScheduler);

            builder.Register(_ => {
                var environmentManager = new EnvironmentManager(new EnvironmentResolver());
                environmentManager.RegisterLateUpdatable(LateUpdateOrder.Vfx);
                return environmentManager;
            }, Lifetime.Singleton);

            builder.Register(_ => {
                var assetManager = new AssetManager();
                assetManager.Initialize(new AddressablesAssetProvider(), new ResourcesAssetProvider(), new AssetDatabaseAssetProvider());
                return assetManager;
            }, Lifetime.Singleton);

            builder.Register(resolver => {
                var uiManager = new UIManager();
                uiManager.Initialize(new UIAssetLoader(resolver.Resolve<AssetManager>()));
                uiManager.RegisterUpdatable(UpdateOrder.UI);
                uiManager.RegisterLateUpdatable(LateUpdateOrder.UI);
                ResidentUIUtility.Initialize(uiManager);
                DialogUIUtility.Initialize(uiManager);
                return uiManager;
            }, Lifetime.Singleton);

            var appNavigator = new AppNavigator();
            builder.RegisterInstance<IAppNavigator>(appNavigator);
            appNavigator.RegisterUpdatable(UpdateOrder.Controller);

            _globalResolver = builder.Build().RegisterTo(_globalScope);
            
            // AppNavigator初期化
            appNavigator.RegisterTo(_globalScope);
            appNavigator.Initialize(_globalResolver);

            // DeltaTimeProvider初期化
            LayeredTime.DefaultProvider = new UnityDeltaTimeProvider();
        }

        /// <inheritdoc/>
        protected override IEnumerator StartRoutineInternal(object[] args) {
            var startArgs = ParseStartArgs(args);

            // Addressables初期化
            yield return Addressables.InitializeAsync();

            // 常駐UI読み込み
            yield return _globalResolver.Resolve<UIManager>().LoadPrefabAsync("resident");

            // Debug初期化
            SetupDebug();

            // 開始Nodeへの遷移
            _globalResolver.Resolve<IAppNavigator>().TransitionTo(startArgs.NodeId, false, startArgs.SetupAction);
        }

        /// <inheritdoc/>
        protected override IEnumerator RebootRoutineInternal(object[] args) {
            var rebootArgs = ParseRebootArgs(args);

            // 開始Nodeにリセット遷移
            _globalResolver.Resolve<IAppNavigator>().TransitionTo(rebootArgs.NodeId, true, rebootArgs.SetupAction);

            yield break;
        }

        /// <summary>
        /// Start時引数の解析
        /// </summary>
        private StartArgs ParseStartArgs(object[] args) {
            var startArgs = new StartArgs();
            if (args.Length > 0) {
                startArgs = (StartArgs)args[0];
            }
            else {
                startArgs.NodeId = AppNavigator.Id.TitleTop;
                startArgs.SetupAction = null;
            }

            return startArgs;
        }

        /// <summary>
        /// Reboot時引数の解析
        /// </summary>
        private RebootArgs ParseRebootArgs(object[] args) {
            var rebootArgs = new RebootArgs();
            if (args.Length > 0) {
                rebootArgs = (RebootArgs)args[0];
            }
            else {
                rebootArgs.NodeId = AppNavigator.Id.TitleTop;
                rebootArgs.SetupAction = null;
            }

            return rebootArgs;
        }

        /// <summary>
        /// Update処理
        /// </summary>
        private void Update() {
            _updateScheduler.Update();
        }

        /// <summary>
        /// LateUpdate処理
        /// </summary>
        private void LateUpdate() {
            _updateScheduler.LateUpdate();
        }

        /// <summary>
        /// FixedUpdate処理
        /// </summary>
        private void FixedUpdate() {
            _updateScheduler.FixedUpdate();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnDestroy() {
            OnApplicationQuit();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnApplicationQuit() {
            CleanupDebug();
            _globalScope.Dispose();
        }
    }
}