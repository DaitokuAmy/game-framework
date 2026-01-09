using Cysharp.Threading.Tasks;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// UITest関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToUITest() {
            //var (transition, effects) = GetDefaultTransitionInfo<UITestSessionNode>();
            //await _engine.TransitionTo<UITestTopScreenNode>(transition, effects);
        }
    }
}