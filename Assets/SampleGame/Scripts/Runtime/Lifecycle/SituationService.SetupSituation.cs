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
        private void SetupContainer(IScope scope) {
            var mainSituation = new MainSituation();
            
            SetupIntroductionSituations(mainSituation);
            SetupBattleSituations(mainSituation);
            SetupViewerSituations(mainSituation);
            
            _situationContainer.Setup(mainSituation);
        }

        /// <summary>
        /// 遷移関係の初期化
        /// </summary>
        private void SetupTree(SituationTree tree, IScope scope) {
            if (tree == null) {
                return;
            }
            
            var titleTopNode = tree.ConnectRoot<TitleTopSituation>();
            var titleOptionNode = titleTopNode.Connect<TitleOptionSituation>();
            var modelViewerSceneNode = titleTopNode.Connect<ModelViewerSceneSituation>();
            var battleSceneNode = titleTopNode.Connect<BattleSceneSituation>();
            var battlePauseNode = battleSceneNode.Connect<BattlePauseSituation>();
            tree.SetFallbackNode(modelViewerSceneNode);
            tree.SetFallbackNode(battleSceneNode);
        }
    }
}