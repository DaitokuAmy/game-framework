using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Viewer関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <inheritdoc/>
        IProcess ISituationService.TransitionModelViewer() {
            return Transition<SortieTopSituation>(transitionType: TransitionType.SceneDefault);
        }
        
        /// <summary>
        /// Viewer関連のSituationの初期化
        /// </summary>
        private void SetupViewerSituations(Situation parentSituation) {
            var modelViewerSituation = new ModelViewerSceneSituation();
            
            modelViewerSituation.SetParent(parentSituation);
        }

        /// <summary>
        /// Viewer関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupViewerTreeNodes(StateTreeNode<Type> parentNode) {
            var modelViewerNode = ConnectNode<ModelViewerSceneSituation>(parentNode);
            return modelViewerNode;
        }
    }
}