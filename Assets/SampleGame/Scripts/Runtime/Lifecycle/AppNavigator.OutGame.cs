using Cysharp.Threading.Tasks;
using GameFramework.NavigationSystem;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// OutGame関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieTop() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieTopScreenNode>(Id.SortieTop, transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieRoleSelect() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieRoleSelectScreenNode>(Id.SortieRoleSelectTop, transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieRoleInformation() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieRoleInformationScreenNode>(Id.SortieRoleInformation, transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieMissionSelect() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieMissionSelectScreenNode>(Id.SortieMissionSelect, transition, effects);
        }

        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToSortieDifficultySelect() {
            var (transition, effects) = GetDefaultTransitionInfo<OutGameSessionNode>();
            await _engine.TransitionTo<SortieDifficultySelectScreenNode>(Id.SortieDifficultySelect, transition, effects);
        }

        /// <summary>
        /// OutGameのLifecycle構築
        /// </summary>
        private void SetupOutGameLifecycle(SessionNodeBuilder outGame) {
            outGame.AddScreen<SortieScreenNode>(Id.Sortie, sortie => {
                sortie.AddScreen<SortieTopScreenNode>(Id.SortieTop)
                    .AddScreen<SortieRoleSelectScreenNode>(Id.SortieRoleSelectTop, sortieRoleSelect => {
                        sortieRoleSelect.AddScreen<SortieRoleInformationScreenNode>(Id.SortieRoleInformation);
                    })
                    .AddScreen<SortieMissionSelectScreenNode>(Id.SortieMissionSelect, sortieMissionSelect => {
                        sortieMissionSelect.AddScreen<SortieDifficultySelectScreenNode>(Id.SortieDifficultySelect);
                    });
            });
        }

        /// <summary>
        /// OutGameの出撃Top画面以降の遷移ツリー
        /// </summary>
        /// <param name="sortieTop"></param>
        private void ConnectOutGameSortieTopTreeNode(NavNodeTreeRouterNodeBuilder sortieTop) {
            sortieTop.Connect(Id.SortieRoleSelectTop, sortieRoleSelect => {
                    sortieRoleSelect.Connect(Id.SortieRoleInformation);
                })
                .Connect(Id.SortieMissionSelect, sortieMissionSelect => {
                    sortieMissionSelect.Connect(Id.SortieDifficultySelect, sortieDifficultySelect => {
                        //sortieDifficultySelect.Connect(Id.BattleHud);
                    });
                })
                .SetGlobalShortcut();
        }
    }
}
