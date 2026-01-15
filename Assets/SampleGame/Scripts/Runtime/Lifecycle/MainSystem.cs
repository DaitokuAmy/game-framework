using System;
using System.Collections;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.BootSystems;
using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using GameFramework.NavigationSystems;
using GameFramework.UISystems;
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
            public Type NavNodeType;
            public Action<INavNode> SetupAction;
        }

        /// <summary>
        /// リブート時の引数
        /// </summary>
        public struct RebootArgs {
            public Type NavNodeType;
            public Action<INavNode> SetupAction;
        }

        private DisposableScope _globalScope;
        private IObjectResolver _globalResolver;
        private TaskRunner _taskRunner;

        /// <inheritdoc/>
        protected override void PreStartInternal(object[] args) {
            _globalScope = new DisposableScope();

            var builder = new ContainerBuilder();

            _taskRunner = new TaskRunner().RegisterTo(_globalScope);
            builder.RegisterInstance(_taskRunner);
            TaskUtility.Initialize(_taskRunner);

            builder.Register(_ => {
                var environmentManager = new EnvironmentManager(new EnvironmentResolver());
                environmentManager.RegisterTask(TaskOrder.PostSystem);
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
                uiManager.RegisterTask(TaskOrder.UI);
                ResidentUIUtility.Initialize(uiManager);
                DialogUIUtility.Initialize(uiManager);
                return uiManager;
            }, Lifetime.Singleton);

            var appNavigator = new AppNavigator();
            builder.RegisterInstance<IAppNavigator>(appNavigator);
            appNavigator.RegisterTo(_globalScope);
            appNavigator.RegisterTask(TaskOrder.PreLogic);

            _globalResolver = builder.Build().RegisterTo(_globalScope);
            
            // AppNavigator初期化
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
            _globalResolver.Resolve<IAppNavigator>().TransitionTo(startArgs.NavNodeType, false, startArgs.SetupAction);
        }

        /// <inheritdoc/>
        protected override IEnumerator RebootRoutineInternal(object[] args) {
            var rebootArgs = ParseRebootArgs(args);

            // 開始Nodeにリセット遷移
            _globalResolver.Resolve<IAppNavigator>().TransitionTo(rebootArgs.NavNodeType, true, rebootArgs.SetupAction);

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
                startArgs.NavNodeType = typeof(TitleTopScreenNode);
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
                rebootArgs.NavNodeType = typeof(TitleTopScreenNode);
                rebootArgs.SetupAction = null;
            }

            return rebootArgs;
        }

        /// <summary>
        /// Update処理
        /// </summary>
        private void Update() {
            _taskRunner.Update();
        }

        /// <summary>
        /// LateUpdate処理
        /// </summary>
        private void LateUpdate() {
            _taskRunner.LateUpdate();
        }

        /// <summary>
        /// FixedUpdate処理
        /// </summary>
        private void FixedUpdate() {
            _taskRunner.FixedUpdate();
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