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
            public TransitionType TransitionType;
        }

        /// <summary>
        /// リブート時の引数
        /// </summary>
        public struct RebootArgs {
            public Type NavNodeType;
            public Action<INavNode> SetupAction;
            public TransitionType TransitionType;
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
            
            var environmentManager = new EnvironmentManager(new EnvironmentResolver()).RegisterTo(_globalScope);
            builder.RegisterInstance(environmentManager);
            environmentManager.RegisterTask(TaskOrder.PostSystem);
            
            var assetManager = new AssetManager();
            builder.RegisterInstance(assetManager);
            assetManager.Initialize(new AddressablesAssetProvider(), new ResourcesAssetProvider(), new AssetDatabaseAssetProvider());
            
            var uiManager = new UIManager();
            builder.RegisterInstance(uiManager);
            uiManager.Initialize(new UIAssetLoader(assetManager));
            uiManager.RegisterTask(TaskOrder.UI);
            ResidentUIUtility.Initialize(uiManager);
            DialogUIUtility.Initialize(uiManager);

            builder.Register<IAppNavigator, AppNavigator>(Lifetime.Singleton);
            
            _globalResolver = builder.Build().RegisterTo(_globalScope);

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
            _globalResolver.Resolve<IAppNavigator>().TransitionTo(startArgs.NavNodeType, false, startArgs.SetupAction, startArgs.TransitionType);
        }

        /// <inheritdoc/>
        protected override IEnumerator RebootRoutineInternal(object[] args) {
            var rebootArgs = ParseRebootArgs(args);

            // 開始Nodeにリセット遷移
            _globalResolver.Resolve<IAppNavigator>().TransitionTo(rebootArgs.NavNodeType, true, rebootArgs.SetupAction, rebootArgs.TransitionType);
            
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
                startArgs.TransitionType = TransitionType.SceneDefault;
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
                rebootArgs.TransitionType = TransitionType.SceneDefault;
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