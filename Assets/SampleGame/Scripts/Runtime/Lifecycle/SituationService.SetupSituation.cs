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
            
            var titleTopNode = treeRouter.ConnectRoot(typeof(TitleTopSituation));
            var titleOptionNode = titleTopNode.Connect(typeof(TitleOptionSituation));
            var modelViewerSceneNode = titleTopNode.Connect(typeof(ModelViewerSceneSituation));
            var battleSceneNode = titleTopNode.Connect(typeof(BattleSceneSituation));
            var battlePauseNode = battleSceneNode.Connect(typeof(BattlePauseSituation));
            treeRouter.SetFallbackNode(modelViewerSceneNode);
            treeRouter.SetFallbackNode(battleSceneNode);
            
            treeRouter.SetFallbackNode(titleOptionNode, titleTopNode);
            treeRouter.SetFallbackNode(battlePauseNode, battleSceneNode);
        }
    }
}