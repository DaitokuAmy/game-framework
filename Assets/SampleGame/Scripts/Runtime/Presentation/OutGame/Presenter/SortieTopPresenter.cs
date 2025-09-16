using GameFramework;
using GameFramework.Core;
using SampleGame.Application;
using R3;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// 出撃画面トップ用のPresenter
    /// </summary>
    public class SortieTopPresenter : UIScreenLogic<SortieTopUIScreen>, IServiceUser {
        private ISituationService _situationService;
        
        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver serviceResolver) {
            _situationService = serviceResolver.Resolve<ISituationService>();
        }
        
        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.SelectedIndexSubject
                .TakeUntil(scope)
                .Subscribe(idx => {
                    if (idx == 0) {
                        _situationService.TransitionSortieMissionSelect();
                    }
                });
        }
    }
}