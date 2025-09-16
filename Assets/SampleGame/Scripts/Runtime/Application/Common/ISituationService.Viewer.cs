using GameFramework.Core;

namespace SampleGame.Application {
    /// <summary>
    /// ViewerのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface ISituationService {
        /// <summary>
        /// モデルビューアーへの遷移
        /// </summary>
        IProcess TransitionModelViewer();
    }
}