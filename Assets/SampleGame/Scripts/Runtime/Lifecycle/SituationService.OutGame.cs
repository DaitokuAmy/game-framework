using System;
using GameFramework;
using GameFramework.SituationSystems;
using SampleGame.Presentation.Battle;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// OutGame関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// OutGame関連のSituationの初期化
        /// </summary>
        private void SetupOutGameSituations(Situation parentSituation) {
            var outGameSceneSituation = new OutGameSceneSituation();
            outGameSceneSituation.SetParent(parentSituation);
            
            var sortieSituation = new SortieSituation();
            sortieSituation.SetParent(outGameSceneSituation);
            var sortieTopSituation = new SortieTopSituation();
            sortieTopSituation.SetParent(sortieSituation);
            var sortieRoleSelectSituation = new SortieRoleSelectSituation();
            sortieRoleSelectSituation.SetParent(sortieSituation);
            var sortieRoleInformationSituation = new SortieRoleInformationSituation();
            sortieRoleInformationSituation.SetParent(sortieRoleSelectSituation);
            var sortieMissionSelectSituation = new SortieMissionSelectSituation();
            sortieMissionSelectSituation.SetParent(sortieSituation);
            var sortieDifficultySelectSituation = new SortieDifficultySelectSituation();
            sortieDifficultySelectSituation.SetParent(sortieMissionSelectSituation);
        }

        /// <summary>
        /// OutGame関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupOutGameTreeNodes(StateTreeNode<Type> parentNode) {
            var sortieTopNode = ConnectNode<SortieTopSituation>(parentNode);
            var roleSelectNode = ConnectNode<SortieRoleSelectSituation>(sortieTopNode);
            var roleInformationNode = ConnectNode<SortieRoleInformationSituation>(roleSelectNode);
            var missionSelectNode = ConnectNode<SortieMissionSelectSituation>(sortieTopNode);
            var difficultySelectNode = ConnectNode<SortieDifficultySelectSituation>(missionSelectNode);
            return sortieTopNode;
        }
    }
}