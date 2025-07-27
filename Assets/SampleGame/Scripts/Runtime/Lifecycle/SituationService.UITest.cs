using System;
using GameFramework;
using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// UITest関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// UITest関連のSituationの初期化
        /// </summary>
        private void SetupUITestSituations(Situation parentSituation) {
            var uiTestSceneSituation = new UITestSceneSituation();
            uiTestSceneSituation.SetParent(parentSituation);
        }

        /// <summary>
        /// UITest関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupUITestTreeNodes(StateTreeNode<Type> parentNode) {
            var uiTestNode = ConnectNode<UITestSceneSituation>(parentNode);
            return uiTestNode;
        }
    }
}