using System.Collections;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Introduction;
using R3;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// TitleTop用のSituation
    /// </summary>
    public class TitleTopSituation : Situation {
        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);

            var uiManager = Services.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();
            yield return introductionUIService.TitleTopUIScreen.OpenAsync();
        }

        /// <summary>
        /// 開きが完了する処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle handle, IScope scope) {
            base.PostOpenInternal(handle, scope);

            var uiManager = Services.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();
            introductionUIService.TitleTopUIScreen.OpenAsync(immediate: true);
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.CloseRoutineInternal(handle, animationScope);

            var uiManager = Services.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();
            yield return introductionUIService.TitleTopUIScreen.CloseAsync();
        }

        /// <summary>
        /// 閉じが完了する処理
        /// </summary>
        protected override void PostCloseInternal(TransitionHandle handle) {
            base.PostCloseInternal(handle);

            var uiManager = Services.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();
            introductionUIService.TitleTopUIScreen.CloseAsync(immediate: true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var situationService = Services.Resolve<SituationService>();
            var uiManager = Services.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();

            // スタートボタン
            introductionUIService.TitleTopUIScreen.ClickedStartButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.Transition<BattleSceneSituation>(transitionType: SituationService.TransitionType.SceneDefault); });

            // オプションボタン
            introductionUIService.TitleTopUIScreen.ClickedOptionButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.Transition<TitleOptionSituation>(transitionType: SituationService.TransitionType.ScreenCross); });

            // モデルビューアーボタン
            introductionUIService.TitleTopUIScreen.ClickedModelViewerButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.Transition<ModelViewerSceneSituation>(transitionType: SituationService.TransitionType.SceneDefault); });
        }
    }
}