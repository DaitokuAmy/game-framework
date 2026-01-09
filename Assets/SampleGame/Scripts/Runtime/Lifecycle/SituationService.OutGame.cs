using Cysharp.Threading.Tasks;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// OutGame関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieTop() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieTopScreenNode>(transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieRoleSelect() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieRoleSelectScreenNode>(transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieRoleInformation() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieRoleInformationScreenNode>(transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieMissionSelect() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieMissionSelectScreenNode>(transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieDifficultySelect() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieDifficultySelectScreenNode>(transition, effects);
        }
    }
}