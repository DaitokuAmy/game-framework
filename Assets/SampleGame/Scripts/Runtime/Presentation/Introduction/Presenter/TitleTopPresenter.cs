using GameFramework;
using GameFramework;
using SampleGame.Application;
using R3;
using SampleGame.Presentation.Introduction;
using VContainer;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// タイトルトップ用のPresenter
    /// </summary>
    public class TitleTopPresenter : UIScreenLogic<TitleTopUIScreen> {
        [Inject]
        private IAppNavigator _appNavigator;

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.ClickedStartButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.TransitionToSortieTop();
                });
            Screen.ClickedOptionButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.TransitionToTitleOption();
                });
            Screen.ClickedUITestButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.TransitionToUITest();
                });
            Screen.ClickedModelViewerButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.TransitionToModelViewer();
                });
        }
    }
}