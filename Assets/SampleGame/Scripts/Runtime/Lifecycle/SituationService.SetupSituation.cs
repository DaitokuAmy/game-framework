using GameFramework.Core;
using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// SituationService
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// Situationコンテナの初期化
        /// </summary>
        private void SetupContainer(SituationContainer container, IScope scope) {
            var mainSituation = new MainSituation();
            
            SetupIntroductionSituations(mainSituation);
            SetupBattleSituations(mainSituation);
            SetupUITestSituations(mainSituation);
            SetupViewerSituations(mainSituation);
            
            container.Setup(mainSituation);
        }

        /// <summary>
        /// 遷移関係の初期化
        /// </summary>
        private void SetupTree(SituationTreeRouter treeRouter, IScope scope) {
            if (treeRouter == null) {
                return;
            }

            var titleTopNode = SetupIntroductionTreeNodes(null);
            var battleNode = SetupBattleTreeNodes(titleTopNode);
            var uiTestNode = SetupUITestTreeNodes(titleTopNode);
            var modelViewerNode = SetupViewerTreeNodes(titleTopNode);
            
            treeRouter.SetFallbackNode(modelViewerNode);
            treeRouter.SetFallbackNode(battleNode);
            treeRouter.SetFallbackNode(uiTestNode);
        }
    }
}