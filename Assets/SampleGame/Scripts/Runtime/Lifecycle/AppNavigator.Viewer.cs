using Cysharp.Threading.Tasks;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Viewer関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        UniTask IAppNavigator.TransitionToModelViewer() {
            // var (transition, effects) = GetDefaultTransitionInfo<ModelViewerSessionNode>();
            // await _engine.TransitionTo<ModelViewerTopScreenNode>(transition, effects);
            return UniTask.CompletedTask;
        }
    }
}