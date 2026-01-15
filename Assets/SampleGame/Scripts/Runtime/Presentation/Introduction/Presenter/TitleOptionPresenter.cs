using GameFramework;
using GameFramework.Core;
using SampleGame.Application;
using R3;
using SampleGame.Presentation.Introduction;
using VContainer;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// タイトルオプション用のPresenter
    /// </summary>
    public class TitleOptionPresenter : UIScreenLogic<TitleOptionUIScreen> {
        [Inject]
        private IAppNavigator _appNavigator;

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.ClickedBackButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.Back();
                });
        }
    }
}