using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// UITest関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <inheritdoc/>
        IProcess ISituationService.TransitionUITest() {
            return Transition<UITestSceneSituation>(transitionType: TransitionType.SceneDefault);
        }
        
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