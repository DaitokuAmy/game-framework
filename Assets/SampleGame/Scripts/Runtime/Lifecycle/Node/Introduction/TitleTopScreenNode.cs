using System.Collections.Generic;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Introduction;
using R3;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// TitleTop用のScreenNode
    /// </summary>
    public class TitleTopScreenNode : ScreenNode<IntroductionUIService> {
        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void Activate(TransitionHandle<INavNode> handle, IScope scope) {
            base.Activate(handle, scope);

            // スタートボタン
            UIService.TitleTopUIScreen.ClickedStartButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { AppNavigator.TransitionSortieTop(); });

            // オプションボタン
            UIService.TitleTopUIScreen.ClickedOptionButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { AppNavigator.TransitionTitleOption(); });

            // モデルビューアーボタン
            UIService.TitleTopUIScreen.ClickedModelViewerButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { AppNavigator.TransitionModelViewer(); });

            // UITestボタン
            UIService.TitleTopUIScreen.ClickedUITestButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => { AppNavigator.TransitionUITest(); });
        }

        /// <inheritdoc/>
        protected override void GetScreens(IntroductionUIService service, List<UIScreen> screens) {
            screens.Add(service.TitleTopUIScreen);
        }
    }
}