using Cysharp.Threading.Tasks;

namespace SampleGame.Application {
    /// <summary>
    /// ViewerのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface IAppNavigator {
        /// <summary>
        /// モデルビューアーへの遷移
        /// </summary>
        UniTask TransitionToModelViewer();
    }
}