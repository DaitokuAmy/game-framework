using GameFramework;
using SampleGame.Application;
using R3;
using VContainer;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面ベース用のPresenter
    /// </summary>
    public class SortieBasePresenter : UIScreenLogic<SortieBaseUIScreen> {
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