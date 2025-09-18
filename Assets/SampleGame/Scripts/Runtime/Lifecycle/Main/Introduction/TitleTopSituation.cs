using System.Collections;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Introduction;
using R3;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// TitleTop用のSituation
    /// </summary>
    public class TitleTopSituation : Situation {
        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);

            var uiManager = ServiceResolver.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();
            yield return introductionUIService.TitleTopUIScreen.OpenAsync();
        }

        /// <summary>
        /// 開きが完了する処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle<Situation> handle, IScope scope) {
            base.PostOpenInternal(handle, scope);

            var uiManager = ServiceResolver.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();
            introductionUIService.TitleTopUIScreen.OpenAsync(immediate: true);
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield return base.CloseRoutineInternal(handle, animationScope);

            var uiManager = ServiceResolver.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();
            yield return introductionUIService.TitleTopUIScreen.CloseAsync();
        }

        /// <summary>
        /// 閉じが完了する処理
        /// </summary>
        protected override void PostCloseInternal(TransitionHandle<Situation> handle) {
            base.PostCloseInternal(handle);

            var uiManager = ServiceResolver.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();
            introductionUIService.TitleTopUIScreen.CloseAsync(immediate: true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var situationService = ServiceResolver.Resolve<ISituationService>();
            var uiManager = ServiceResolver.Resolve<UIManager>();
            var introductionUIService = uiManager.GetService<IntroductionUIService>();

            // スタートボタン
            introductionUIService.TitleTopUIScreen.ClickedStartButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.TransitionSortieTop(); });

            // オプションボタン
            introductionUIService.TitleTopUIScreen.ClickedOptionButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.TransitionTitleOption(); });

            // モデルビューアーボタン
            introductionUIService.TitleTopUIScreen.ClickedModelViewerButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.TransitionModelViewer(); });

            // UITestボタン
            introductionUIService.TitleTopUIScreen.ClickedUITestButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { situationService.TransitionUITest(); });
        }
    }
}