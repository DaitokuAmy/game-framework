using System;
using System.Collections;
using GameFramework;
using GameFramework.AssetSystems;
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
    public partial class MainSystem : MainSystem<MainSystem> {
        [SerializeField]
        private ServiceContainerInstaller _globalObject;

        private TaskRunner _taskRunner;
        private SituationService _situationService;
        private IServiceContainer _globalServiceContainer;

        /// <summary>
        /// Reboot処理
        /// </summary>
        protected override IEnumerator RebootRoutineInternal(object[] args) {
            // Scene用のContainerの作成しなおし
            _globalServiceContainer.Remove<SituationService>();
            _situationService = new SituationService(_globalServiceContainer);
            _situationService.Initialize();
            _situationService.RegisterTask(TaskOrder.Logic);
            _globalServiceContainer.RegisterInstance<ISituationService>(_situationService);

            // 開始Situationへの遷移
            yield return TransitionStartSituation(args);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator StartRoutineInternal(object[] args) {
            _globalServiceContainer = new ServiceContainer(label: "Global");
            
            // FPS初期化
            UnityEngine.Application.targetFrameRate = 60;
            
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

            // Addressables初期化
            yield return Addressables.InitializeAsync();

            var assetManager = new AssetManager();
            assetManager.Initialize(new AddressablesAssetProvider(), new ResourcesAssetProvider(), new AssetDatabaseAssetProvider());
            _globalServiceContainer.RegisterInstance(assetManager);

            var uiManager = new UIManager();
            uiManager.Initialize(new UIAssetLoader(assetManager));
            uiManager.RegisterTask(TaskOrder.UI);
            _globalServiceContainer.RegisterInstance(uiManager);

            // SituationServiceの初期化
            _situationService = new SituationService(_globalServiceContainer);
            _situationService.Initialize();
            _situationService.RegisterTask(TaskOrder.Logic);
            _globalServiceContainer.RegisterInstance<ISituationService>(_situationService);

            // Debug初期化
            SetupDebug();

            // 常駐UIの読み込み
            yield return uiManager.LoadPrefabAsync("resident");
            
            // Utility初期化
            ResidentUIUtility.Initialize(_globalServiceContainer);
            DialogUIUtility.Initialize(_globalServiceContainer);

            // 開始Situationへの遷移
            yield return TransitionStartSituation(args);
        }

        /// <summary>
        /// Update処理
        /// </summary>
        protected override void UpdateInternal() {
            _taskRunner.Update();
        }

        /// <summary>
        /// LateUpdate処理
        /// </summary>
        protected override void LateUpdateInternal() {
            _taskRunner.LateUpdate();
        }

        /// <summary>
        /// FixedUpdate処理
        /// </summary>
        protected override void FixedUpdateInternal() {
            _taskRunner.FixedUpdate();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        protected override void OnDestroyInternal() {
            OnApplicationQuit();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        private void OnApplicationQuit() {
            CleanupDebug();
            _globalServiceContainer.Dispose();
        }

        /// <summary>
        /// 開始Situationへ遷移する
        /// </summary>
        private TransitionHandle<Situation> TransitionStartSituation(object[] args) {
            ParseArguments(args, out var situationType, out var onSetup);
            return _situationService.Transition(situationType, onSetup, SituationService.TransitionType.SceneDefault);
        }

        /// <summary>
        /// 引数をパースする
        /// </summary>
        private void ParseArguments(object[] args, out Type situationType, out Action<Situation> onSetup) {
            situationType = typeof(TitleTopSituation);
            onSetup = null;

            // Entry経由
            if (args.Length <= 0) {
                return;
            }

            // StarterのSituation指定あり
            if (args[0] is ISituationSetup situationSetup) {
                situationType = situationSetup.SituationType;
                onSetup = situationSetup.OnSetup;
            }
        }
    }
}