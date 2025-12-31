using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.BootSystems;
using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Application;
using SampleGame.Infrastructure;
using SampleGame.Presentation;
using ThirdPersonEngine;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
            public Type SituationType;
            public Action<Situation> SetupAction;
            public SituationService.TransitionType TransitionType;
        }

        /// <summary>
        /// リブート時の引数
        /// </summary>
        public struct RebootArgs {
            public Type SituationType;
            public Action<Situation> SetupAction;
            public SituationService.TransitionType TransitionType;
        }

        [SerializeField]
        private ServiceContainerInstaller _globalObject;

        private TaskRunner _taskRunner;
        private SituationService _situationService;
        private IServiceContainer _globalServiceContainer;

        /// <inheritdoc/>
        protected override void PreStartInternal(object[] args) {
            _globalServiceContainer = new ServiceContainer(label: "Global");

            // DeltaTimeProvider初期化
            LayeredTime.DefaultProvider = new UnityDeltaTimeProvider();

            // RootのServiceにインスタンスを登録
            _globalObject.Install(_globalServiceContainer);
            DontDestroyOnLoad(_globalObject.gameObject);

            // 各種システム初期化
            _taskRunner = new TaskRunner();
            _globalServiceContainer.RegisterInstance(_taskRunner);
            TaskUtility.Initialize(_globalServiceContainer);

            var environmentManager = new EnvironmentManager(new EnvironmentResolver());
            environmentManager.RegisterTask(TaskOrder.PostSystem);
            _globalServiceContainer.RegisterInstance(environmentManager);
        }

        /// <inheritdoc/>
        protected override IEnumerator StartRoutineInternal(object[] args) {
            var startArgs = ParseStartArgs(args);

            // Addressables初期化
            yield return Addressables.InitializeAsync();

            var assetManager = new AssetManager();
            assetManager.Initialize(new AddressablesAssetProvider(), new ResourcesAssetProvider(), new AssetDatabaseAssetProvider());
            _globalServiceContainer.RegisterInstance(assetManager);

            var uiManager = new UIManager();
            uiManager.Initialize(new UIAssetLoader(assetManager));
            uiManager.RegisterTask(TaskOrder.UI);
            _globalServiceContainer.RegisterInstance(uiManager);
            
            // 常駐UI読み込み
            yield return uiManager.LoadPrefabAsync("resident");

            // SituationServiceの初期化
            _situationService = new SituationService(_globalServiceContainer);
            _situationService.Initialize();
            _situationService.RegisterTask(TaskOrder.Logic);
            _globalServiceContainer.RegisterInstance<ISituationService>(_situationService);

            // Debug初期化
            SetupDebug();

            // Utility初期化
            ResidentUIUtility.Initialize(_globalServiceContainer);
            DialogUIUtility.Initialize(_globalServiceContainer);

            // 開始Situationへの遷移
            _situationService.Transition(startArgs.SituationType, startArgs.SetupAction, startArgs.TransitionType);
        }

        /// <inheritdoc/>
        protected override IEnumerator RebootRoutineInternal(object[] args) {
            var rebootArgs = ParseRebootArgs(args);
            
            // Scene用のContainerの作成しなおし
            _globalServiceContainer.Remove<SituationService>();
            _situationService = new SituationService(_globalServiceContainer);
            _situationService.Initialize();
            _situationService.RegisterTask(TaskOrder.Logic);
            _globalServiceContainer.RegisterInstance<ISituationService>(_situationService);

            // 開始Situationへの遷移
            _situationService.Transition(rebootArgs.SituationType, rebootArgs.SetupAction, rebootArgs.TransitionType);
            
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
                startArgs.SituationType = typeof(TitleTopSituation);
                startArgs.SetupAction = null;
                startArgs.TransitionType = SituationService.TransitionType.SceneDefault;
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
                rebootArgs.SituationType = typeof(TitleTopSituation);
                rebootArgs.SetupAction = null;
                rebootArgs.TransitionType = SituationService.TransitionType.SceneDefault;
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
            _globalServiceContainer.Dispose();
        }
    }
}