using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <inheritdoc/>
        IProcess ISituationService.TransitionBattle() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<BattleSceneSituation>(transitionType: transitionType);
        }

        /// <inheritdoc/>
        IProcess ISituationService.TransitionBattlePause() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<BattlePauseSituation>(transitionType: transitionType);
        }

        /// <summary>
        /// Battle関連のSituationの初期化
        /// </summary>
        private void SetupBattleSituations(Situation parentSituation) {
            var battleSceneSituation = new BattleSceneSituation();
            battleSceneSituation.SetParent(parentSituation);
            var battlePauseSituation = new BattlePauseSituation();
            battlePauseSituation.SetParent(battleSceneSituation);
        }

        /// <summary>
        /// Battle関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupBattleTreeNodes(StateTreeNode<Type> parentNode) {
            var battleNode = ConnectNode<BattleSceneSituation>(parentNode);
            var battlePauseNode = ConnectNode<BattlePauseSituation>(battleNode);
            var optionNode = ConnectNode<OptionSituation>(battlePauseNode);
            return battleNode;
        }
    }
}