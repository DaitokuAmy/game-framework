using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.ActorSystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Infrastructure;
// using SampleGame.Infrastructure.OutGame;
// using SampleGame.Presentation.OutGame;
// using R3;
// using SampleGame.Application.OutGame;
// using SampleGame.Domain.OutGame;
using SampleGame.Presentation.UITest;
using ThirdPersonEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// OutGame用のSceneSituation
    /// </summary>
    public class OutGameSceneSituation : SceneSituation {
        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/out_game.unity";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);

            // UI読み込み
            var tasks = new List<UniTask>();
            tasks.Add(LoadUIAsync(scope, scope.Token));

            yield return UniTask.WhenAll(tasks).ToCoroutine();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            SetupInfrastructures(scope);
            SetupManagers(scope);
            SetupDomains(scope);
            SetupApplications(scope);
            SetupFactories(scope);

            // 初期化処理
            //yield return Services.Resolve<OutGameAppService>().SetupAsync(_battleId, _playerId, scope.Token).ToCoroutine();
            
            SetupPresentations(scope);
        }

        /// <summary>
        /// クリーンアップ
        /// </summary>
        protected override void CleanupInternal(TransitionHandle<Situation> handle) {
            //Services.Resolve<OutGameAppService>()?.Cleanup();

            base.CleanupInternal(handle);
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);

            // var uiManager = Services.Resolve<UIManager>();
            // var battleHudUIService = uiManager.GetService<OutGameHudUIService>();
            // yield return battleHudUIService.OutGameHudUIScreen.OpenAsync();
        }

        /// <summary>
        /// 開いた後の処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle<Situation> handle, IScope scope) {
            base.PostOpenInternal(handle, scope);

            // var uiManager = Services.Resolve<UIManager>();
            // var battleHudUIService = uiManager.GetService<OutGameHudUIService>();
            // battleHudUIService.OutGameHudUIScreen.OpenAsync(immediate: true);
        }

        /// <summary>
        /// 閉く処理
        /// </summary>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield return base.CloseRoutineInternal(handle, animationScope);

            // var uiManager = Services.Resolve<UIManager>();
            // var battleHudUIService = uiManager.GetService<OutGameHudUIService>();
            // yield return battleHudUIService.OutGameHudUIScreen.CloseAsync();
        }

        /// <summary>
        /// 閉じた後の処理
        /// </summary>
        protected override void PostCloseInternal(TransitionHandle<Situation> handle) {
            base.PostCloseInternal(handle);

            // var uiManager = Services.Resolve<UIManager>();
            // var battleHudUIService = uiManager.GetService<OutGameHudUIService>();
            // battleHudUIService.OutGameHudUIScreen.CloseAsync(immediate: true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
            base.ActivateInternal(handle, scope);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            var uiManager = Services.Resolve<UIManager>();

            UniTask LoadAsync(string assetKey) {
                return uiManager.LoadSceneAsync(assetKey).RegisterTo(unloadScope).ToUniTask(cancellationToken: ct);
            }

            return UniTask.WhenAll(LoadAsync("out_game"));
        }

        /// <summary>
        /// Infrastructure初期化
        /// </summary>
        private void SetupInfrastructures(IScope scope) {
            ServiceContainer.Register<IModelRepository, ModelRepository>().RegisterTo(scope);
            ServiceContainer.Register<BodyPrefabRepository>().RegisterTo(scope);
            ServiceContainer.Register<EnvironmentSceneRepository>().RegisterTo(scope);
        }

        /// <summary>
        /// Manager初期化
        /// </summary>
        private void SetupManagers(IScope scope) {
            var actorManager = new ActorEntityManager();
            ServiceContainer.RegisterInstance(actorManager).RegisterTo(scope);

            var cameraManager = Services.Resolve<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);
        }

        /// <summary>
        /// Domain初期化
        /// </summary>
        private void SetupDomains(IScope scope) {
            // ServiceContainer.Register<OutGameDomainService>().RegisterTo(scope);
            // ServiceContainer.Register<CharacterDomainService>().RegisterTo(scope);
        }

        /// <summary>
        /// Application初期化
        /// </summary>
        private void SetupApplications(IScope scope) {
            // ServiceContainer.Register<OutGameAppService>().RegisterTo(scope);
        }

        /// <summary>
        /// Factory初期化
        /// </summary>
        private void SetupFactories(IScope scope) {
            // ServiceContainer.Register<ICharacterActorFactory, CharacterActorFactory>().RegisterTo(scope);
            // ServiceContainer.Register<IFieldActorFactory, FieldActorFactory>().RegisterTo(scope);
        }

        /// <summary>
        /// Presentation初期化
        /// </summary>
        private void SetupPresentations(IScope scope) {
            T AddLogic<T>(T logic, bool activate, IScope scp)
                where T : Logic {
                logic.RegisterTask(TaskOrder.Logic);
                logic.RegisterTo(scp);
                if (activate) {
                    logic.Activate();
                }

                return logic;
            }

            // var uiManager = Services.Resolve<UIManager>();
            // var overlayUIService = uiManager.GetService<OutGameOverlayUIService>();
            // overlayUIService.OverlayScreenContainer.RegisterHandler(AddLogic(new OverlayUIScreenPresenter(), false, scope));
            //
            // AddLogic(new CameraPresenter(), true, scope);
        }
    }
}