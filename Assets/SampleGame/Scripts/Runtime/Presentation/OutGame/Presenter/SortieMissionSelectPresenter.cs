using GameFramework;
using GameFramework.Core;
namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// 出撃画面ミッション選択用のPresenter
    /// </summary>
    public class SortieMissionSelectPresenter : UIScreenLogic<SortieMissionSelectUIScreen>, IServiceUser {
        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver serviceResolver) {
        }
        
        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
        }
    }
}