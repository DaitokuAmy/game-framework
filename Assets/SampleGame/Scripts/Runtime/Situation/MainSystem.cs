using System;
using System.Collections;
using GameFramework.AssetSystems;
using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using GameFramework.UISystems;
using SampleGame.Infrastructure;
using SampleGame.Introduction;
using SampleGame.Presentation;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SampleGame {
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
            Services.Instance.Set(_situationService);

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
            Services.Instance.Set(_taskRunner);

            var environmentManager = new EnvironmentManager(new EnvironmentResolver());
            environmentManager.RegisterTask(TaskOrder.PostSystem);
            Services.Instance.Set(environmentManager);

            // Addressables初期化
            yield return Addressables.InitializeAsync();

            var assetManager = new AssetManager();
            assetManager.Initialize(new AddressablesAssetProvider(), new ResourcesAssetProvider(), new AssetDatabaseAssetProvider());
            Services.Instance.Set(assetManager);

            var uiManager = new UIManager();
            uiManager.Initialize(new UIAssetLoader(assetManager));
            uiManager.RegisterTask(TaskOrder.UI);
            Services.Instance.Set(uiManager);

            // SituationServiceの初期化
            _situationService = new SituationService();
            _situationService.Initialize();
            _situationService.RegisterTask(TaskOrder.Logic);
            Services.Instance.Set(_situationService);

            // Debug初期化
            SetupDebug();

            // 常駐UIの読み込み
            yield return uiManager.LoadPrefabAsync("ui_resident");

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
        private IProcess TransitionStartSituation(object[] args) {
            ParseArguments(args, out var situationType, out var onSetup, out var transitionEffects);
            return _situationService.Transition(situationType, onSetup, null, transitionEffects);
        }

        /// <summary>
        /// 引数をパースする
        /// </summary>
        private void ParseArguments(object[] args, out Type situationType, out Action<Situation> onSetup, out ITransitionEffect[] transitionEffects) {
            situationType = typeof(IntroductionSceneSituation);
            onSetup = null;
            transitionEffects = TransitionUtility.CreateLoadingTransitionEffects();

            // Entry経由
            if (args.Length <= 0) {
                return;
            }

            // 遷移効果あり
            if (args.Length >= 2) {
                transitionEffects = args[1] as ITransitionEffect[];
            }

            // StarterのSituation指定あり
            if (args[0] is ISituationSetup situationSetup) {
                situationType = situationSetup.SituationType;
                onSetup = situationSetup.OnSetup;
            }
        }
    }
}