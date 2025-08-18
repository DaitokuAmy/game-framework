using System;
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
using SampleGame.Infrastructure.Battle;
using SampleGame.Presentation.Battle;
using R3;
using SampleGame.Application.Battle;
using SampleGame.Domain.Battle;
using SampleGame.Presentation.UITest;
using ThirdPersonEngine;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle用のSceneSituation
    /// </summary>
    public class BattleSceneSituation : SceneSituation {
        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/battle.unity";

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
            yield return Services.Resolve<BattleAppService>().SetupAsync(1, 1, scope.Token).ToCoroutine();
            
            SetupPresentations(scope);
        }

        /// <summary>
        /// クリーンアップ
        /// </summary>
        protected override void CleanupInternal(TransitionHandle<Situation> handle) {
            Services.Resolve<BattleAppService>()?.Cleanup();

            base.CleanupInternal(handle);
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);

            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            yield return battleHudUIService.BattleHudUIScreen.OpenAsync();
        }

        /// <summary>
        /// 開いた後の処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle<Situation> handle, IScope scope) {
            base.PostOpenInternal(handle, scope);

            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            battleHudUIService.BattleHudUIScreen.OpenAsync(immediate: true);
        }

        /// <summary>
        /// 閉く処理
        /// </summary>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield return base.CloseRoutineInternal(handle, animationScope);

            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            yield return battleHudUIService.BattleHudUIScreen.CloseAsync();
        }

        /// <summary>
        /// 閉じた後の処理
        /// </summary>
        protected override void PostCloseInternal(TransitionHandle<Situation> handle) {
            base.PostCloseInternal(handle);

            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            battleHudUIService.BattleHudUIScreen.CloseAsync(immediate: true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var situationService = Services.Resolve<SituationService>();
            var uiManager = Services.Resolve<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();

            // メニューボタン
            battleHudUIService.BattleHudUIScreen.ClickedMenuButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.Transition<BattlePauseSituation>(); });
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            var battleAppService = Services.Resolve<BattleAppService>();
            battleAppService.UpdateFrame();
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            var uiManager = Services.Resolve<UIManager>();

            UniTask LoadAsync(string assetKey) {
                return uiManager.LoadSceneAsync(assetKey).RegisterTo(unloadScope).ToUniTask(cancellationToken: ct);
            }

            return UniTask.WhenAll(LoadAsync("battle"));
        }

        /// <summary>
        /// Infrastructure初期化
        /// </summary>
        private void SetupInfrastructures(IScope scope) {
            ServiceContainer.Register<IBattleTableRepository, BattleTableRepository>().RegisterTo(scope);
            ServiceContainer.Register<IModelRepository, ModelRepository>().RegisterTo(scope);
            ServiceContainer.Register<BattleCharacterAssetRepository>().RegisterTo(scope);
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
            ServiceContainer.Register<BattleDomainService>().RegisterTo(scope);
            ServiceContainer.Register<CharacterDomainService>().RegisterTo(scope);
        }

        /// <summary>
        /// Application初期化
        /// </summary>
        private void SetupApplications(IScope scope) {
            ServiceContainer.Register<BattleAppService>().RegisterTo(scope);
            ServiceContainer.Register<PlayerAppService>().RegisterTo(scope);
        }

        /// <summary>
        /// Factory初期化
        /// </summary>
        private void SetupFactories(IScope scope) {
            ServiceContainer.Register<ICharacterActorFactory, CharacterActorFactory>().RegisterTo(scope);
            ServiceContainer.Register<IFieldActorFactory, FieldActorFactory>().RegisterTo(scope);
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

            var uiManager = Services.Resolve<UIManager>();
            var overlayUIService = uiManager.GetService<BattleOverlayUIService>();
            overlayUIService.OverlayScreenContainer.RegisterHandler(AddLogic(new OverlayUIScreenPresenter(), false, scope));

            AddLogic(new CameraPresenter(), true, scope);
        }
    }
}