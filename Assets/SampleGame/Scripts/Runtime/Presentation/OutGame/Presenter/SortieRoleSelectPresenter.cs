using GameFramework;
using GameFramework.Core;
namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// 出撃画面兵科選択用のPresenter
    /// </summary>
    public class SortieRoleSelectPresenter : UIScreenLogic<SortieRoleSelectUIScreen>, IServiceUser {
        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver serviceResolver) {
        }
        
        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
        }
    }
}