using GameFramework;
using GameFramework.Core;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面難易度選択用のPresenter
    /// </summary>
    public class SortieDifficultySelectPresenter : UIScreenLogic<SortieDifficultySelectUIScreen>, IServiceUser {
        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver serviceResolver) {
        }
        
        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
        }
    }
}