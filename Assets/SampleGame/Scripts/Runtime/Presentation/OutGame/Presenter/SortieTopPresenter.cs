using GameFramework;
using GameFramework.Core;
using SampleGame.Application;
using R3;
using VContainer;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面トップ用のPresenter
    /// </summary>
    public class SortieTopPresenter : UIScreenLogic<SortieTopUIScreen> {
        [Inject]
        private IAppNavigator _appNavigator;

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.SelectedIndexSubject
                .TakeUntil(scope)
                .Subscribe(idx => {
                    switch (idx) {
                        case 0:
                            _appNavigator.TransitionToSortieMissionSelect();
                            break;
                        case 1:
                            _appNavigator.TransitionToSortieRoleSelect();
                            break;
                    }
                });
        }
    }
}