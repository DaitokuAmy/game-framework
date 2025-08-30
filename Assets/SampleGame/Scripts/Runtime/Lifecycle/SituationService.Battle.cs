using System;
using GameFramework;
using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle関連のSituationService処理
    /// </summary>
    partial class SituationService {
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