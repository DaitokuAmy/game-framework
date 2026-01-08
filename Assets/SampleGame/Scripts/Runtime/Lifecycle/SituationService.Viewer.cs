using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Viewer関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        IProcess IAppNavigator.TransitionModelViewer() {
            return Transition<SortieTopSituation>(transitionType: TransitionType.SceneDefault);
        }
        
        /// <summary>
        /// Viewer関連のSituationの初期化
        /// </summary>
        private void SetupViewerSituations(Situation parentSituation) {
            var modelViewerSituation = new ModelViewerSceneSessionNode();
            
            modelViewerSituation.SetParent(parentSituation);
        }

        /// <summary>
        /// Viewer関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupViewerTreeNodes(StateTreeNode<Type> parentNode) {
            var modelViewerNode = ConnectNode<ModelViewerSceneSessionNode>(parentNode);
            return modelViewerNode;
        }
    }
}