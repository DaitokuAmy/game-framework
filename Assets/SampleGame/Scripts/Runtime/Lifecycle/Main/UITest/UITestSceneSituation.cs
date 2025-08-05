using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.UITest;
using R3;
using SampleGame.Presentation;
using ThirdPersonEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// UITest用のSceneSituation
    /// </summary>
    public class UITestSceneSituation : SceneSituation {
        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/ui_test.unity";

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
            SetupPresentations(scope);
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);

            var uiManager = Services.Resolve<UIManager>();
            var hudUIService = uiManager.GetService<UITestHudUIService>();
            yield return hudUIService.UITestHudUIScreen.OpenAsync();
        }

        /// <summary>
        /// 開いた後の処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle<Situation> handle, IScope scope) {
            base.PostOpenInternal(handle, scope);

            var uiManager = Services.Resolve<UIManager>();
            var hudUIService = uiManager.GetService<UITestHudUIService>();
            hudUIService.UITestHudUIScreen.OpenAsync(immediate: true);
        }

        /// <summary>
        /// 閉く処理
        /// </summary>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield return base.CloseRoutineInternal(handle, animationScope);

            var uiManager = Services.Resolve<UIManager>();
            var hudUIService = uiManager.GetService<UITestHudUIService>();
            yield return hudUIService.UITestHudUIScreen.CloseAsync();
        }

        /// <summary>
        /// 閉じた後の処理
        /// </summary>
        protected override void PostCloseInternal(TransitionHandle<Situation> handle) {
            base.PostCloseInternal(handle);

            var uiManager = Services.Resolve<UIManager>();
            var hudUIService = uiManager.GetService<UITestHudUIService>();
            hudUIService.UITestHudUIScreen.CloseAsync(immediate: true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var uiManager = Services.Resolve<UIManager>();
            var hudUIService = uiManager.GetService<UITestHudUIService>();
            var dialogUIService = uiManager.GetService<DialogUIService>();
            var uiTestDialogUIService = uiManager.GetService<UITestDialogUIService>();

            // メニューボタン
            hudUIService.UITestHudUIScreen.ClickedMenuButtonSubject
                .TakeUntil(scope)
                .SubscribeAwait(async (_, ct) => {
                    // ダイアログを開く
                    var result = await dialogUIService.OpenSelectionDialogAsync(
                        title: "メインメニュー",
                        itemLabels: new[] {
                            "購入ダイアログテスト",
                            "タイトルに戻る"
                        },
                        ct: ct);

                    if (result == 0) {
                        await uiTestDialogUIService.OpenBuyItemDialogAsync(1, ct);
                    }
                    else if (result == 1) {
                        var situationService = Services.Resolve<SituationService>();
                        situationService.Transition<TitleTopSituation>(transitionType: SituationService.TransitionType.SceneDefault);
                    }
                });
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            var uiManager = Services.Resolve<UIManager>();

            UniTask LoadAsync(string assetKey) {
                return uiManager.LoadSceneAsync(assetKey).RegisterTo(unloadScope).ToUniTask(cancellationToken: ct);
            }

            return UniTask.WhenAll(LoadAsync("ui_test"));
        }

        /// <summary>
        /// Infrastructure初期化
        /// </summary>
        private void SetupInfrastructures(IScope scope) {
        }

        /// <summary>
        /// Manager初期化
        /// </summary>
        private void SetupManagers(IScope scope) {
        }

        /// <summary>
        /// Domain初期化
        /// </summary>
        private void SetupDomains(IScope scope) {
        }

        /// <summary>
        /// Application初期化
        /// </summary>
        private void SetupApplications(IScope scope) {
        }

        /// <summary>
        /// Factory初期化
        /// </summary>
        private void SetupFactories(IScope scope) {
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
            var hudUIService = uiManager.GetService<UITestHudUIService>();
            var dialogUIService = uiManager.GetService<UITestDialogUIService>();
            hudUIService.UITestHudUIScreen.RegisterHandler(AddLogic(new HudUIScreenPresenter(), false, scope));
            dialogUIService.SetBuyItemDialogHandler(() => new BuyItemUIDialogPresenter().RegisterTo(scope));
        }
    }
}