using Cysharp.Threading.Tasks;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToBattle() {
            // var (transition, effects) = GetDefaultTransitionInfo<BattleSessionNode>();
            // await _engine.TransitionTo<BattleHudScreenNode>(transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToBattlePause() {
            // var (transition, effects) = GetDefaultTransitionInfo<BattleSessionNode>();
            // await _engine.TransitionTo<BattlePauseScreenNode>(transition, effects);
        }
    }
}