using GameFramework;
using GameFramework.Core;
using SampleGame.Application;
using R3;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面トップ用のPresenter
    /// </summary>
    public class SortieTopPresenter : UIScreenLogic<SortieTopUIScreen> {
        [ServiceInject]
        private ISituationService _situationService;

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.SelectedIndexSubject
                .TakeUntil(scope)
                .Subscribe(idx => {
                    switch (idx) {
                        case 0:
                            _situationService.TransitionSortieMissionSelect();
                            break;
                        case 1:
                            _situationService.TransitionSortieRoleSelect();
                            break;
                        case 2:
                            _situationService.Back();
                            break;
                    }
                });
        }
    }
}