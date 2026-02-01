using GameFramework;
using SampleGame.Application;
using VContainer;
using R3;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面兵科選択用のPresenter
    /// </summary>
    public class SortieRoleSelectPresenter : UIScreenLogic<SortieRoleSelectUIScreen> {
        [Inject]
        private IAppNavigator _appNavigator;

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.SelectedIndexSubject
                .TakeUntil(scope)
                .Subscribe(idx => {
                    switch (idx) {
                        default:
                            _appNavigator.TransitionToSortieRoleInformation();
                            break;
                    }
                });
        }
    }
}