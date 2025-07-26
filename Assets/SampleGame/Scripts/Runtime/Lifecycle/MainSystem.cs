using System;
using System.Collections;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Infrastructure;
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

        /// <summary>
        /// Reboot処理
        /// </summary>
        protected override IEnumerator RebootRoutineInternal(object[] args) {
            // Scene用のContainerの作成しなおし
            Services.Instance.Remove<SituationService>();
            _situationService?.Dispose();
            _situationService = new SituationService();
            _situationService.Initialize();
            _situationService.RegisterTask(TaskOrder.Logic);
            Services.Instance.RegisterInstance(_situationService);

            // 開始Situationへの遷移
            yield return TransitionStartSituation(args);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator StartRoutineInternal(object[] args) {
            // FPS初期化
            UnityEngine.Application.targetFrameRate = 60;

            // RootのServiceにインスタンスを登録
            _globalObject.Install(Services.Instance);
            DontDestroyOnLoad(_globalObject.gameObject);

            // 各種システム初期化
            _taskRunner = new TaskRunner();
            Services.Instance.RegisterInstance(_taskRunner);

            var environmentManager = new EnvironmentManager(new EnvironmentResolver());
            environmentManager.RegisterTask(TaskOrder.PostSystem);
            Services.Instance.RegisterInstance(environmentManager);

            // Addressables初期化
            yield return Addressables.InitializeAsync();

            var assetManager = new AssetManager();
            assetManager.Initialize(new AddressablesAssetProvider(), new ResourcesAssetProvider(), new AssetDatabaseAssetProvider());
            Services.Instance.RegisterInstance(assetManager);

            var uiManager = new UIManager();
            uiManager.Initialize(new UIAssetLoader(assetManager));
            uiManager.RegisterTask(TaskOrder.UI);
            Services.Instance.RegisterInstance(uiManager);

            // SituationServiceの初期化
            _situationService = new SituationService();
            _situationService.Initialize();
            _situationService.RegisterTask(TaskOrder.Logic);
            Services.Instance.RegisterInstance(_situationService);

            // Debug初期化
            SetupDebug();

            // 常駐UIの読み込み
            yield return uiManager.LoadPrefabAsync("resident");

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
            Services.Instance.Clear();
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