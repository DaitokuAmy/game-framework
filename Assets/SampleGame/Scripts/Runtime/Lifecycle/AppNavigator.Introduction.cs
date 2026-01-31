using Cysharp.Threading.Tasks;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Introduction関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToTitleTop() {
            var (transition, effects) = GetDefaultTransitionInfo<IntroductionSessionNode>();
            await _engine.TransitionTo(Id.TitleTop, transition, effects);
        }
        
        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToTitleOption() {
            var (transition, effects) = GetDefaultTransitionInfo<IntroductionSessionNode>();
            await _engine.TransitionTo(Id.TitleOption, transition, effects);
        }
    }
}