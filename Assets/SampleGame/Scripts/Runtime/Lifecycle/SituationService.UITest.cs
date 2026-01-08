using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// UITest関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        IProcess IAppNavigator.TransitionUITest() {
            return Transition<UITestSceneSessionNode>(transitionType: TransitionType.SceneDefault);
        }
        
        /// <summary>
        /// UITest関連のSituationの初期化
        /// </summary>
        private void SetupUITestSituations(Situation parentSituation) {
            var uiTestSceneSituation = new UITestSceneSessionNode();
            uiTestSceneSituation.SetParent(parentSituation);
        }

        /// <summary>
        /// UITest関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupUITestTreeNodes(StateTreeNode<Type> parentNode) {
            var uiTestNode = ConnectNode<UITestSceneSessionNode>(parentNode);
            return uiTestNode;
        }
    }
}