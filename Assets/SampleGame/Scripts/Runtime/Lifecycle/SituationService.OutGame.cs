using System;
using GameFramework;
using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// OutGame関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// OutGame関連のSituationの初期化
        /// </summary>
        private void SetupOutGameSituations(Situation parentSituation) {
            var uiTestSceneSituation = new OutGameSceneSituation();
            uiTestSceneSituation.SetParent(parentSituation);
        }

        /// <summary>
        /// OutGame関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupOutGameTreeNodes(StateTreeNode<Type> parentNode) {
            var sceneNode = ConnectNode<OutGameSceneSituation>(parentNode);
            return sceneNode;
        }
    }
}