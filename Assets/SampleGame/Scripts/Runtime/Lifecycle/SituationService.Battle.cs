using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        IProcess IAppNavigator.TransitionBattle() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSessionNode>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<BattleSceneSessionNode>(transitionType: transitionType);
        }

        /// <inheritdoc/>
        IProcess IAppNavigator.TransitionBattlePause() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSessionNode>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<BattlePauseSituation>(transitionType: transitionType);
        }

        /// <summary>
        /// Battle関連のSituationの初期化
        /// </summary>
        private void SetupBattleSituations(Situation parentSituation) {
            var battleSceneSituation = new BattleSceneSessionNode();
            battleSceneSituation.SetParent(parentSituation);
            var battlePauseSituation = new BattlePauseSituation();
            battlePauseSituation.SetParent(battleSceneSituation);
        }

        /// <summary>
        /// Battle関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupBattleTreeNodes(StateTreeNode<Type> parentNode) {
            var battleNode = ConnectNode<BattleSceneSessionNode>(parentNode);
            var battlePauseNode = ConnectNode<BattlePauseSituation>(battleNode);
            var optionNode = ConnectNode<OptionSituation>(battlePauseNode);
            return battleNode;
        }
    }
}