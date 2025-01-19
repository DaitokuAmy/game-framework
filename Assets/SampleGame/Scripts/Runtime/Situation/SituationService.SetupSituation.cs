using GameFramework.Core;
using SampleGame.Battle;
using SampleGame.Main;
using SampleGame.Introduction;
using SampleGame.ModelViewer;

namespace SampleGame {
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
        private void SetupFlow(IScope scope) {
            var titleTopNode = _situationFlow.ConnectRoot<TitleTopSituation>();
            var titleOptionNode = titleTopNode.Connect<TitleOptionSituation>();
            var modelViewerSceneNode = titleTopNode.Connect<ModelViewerSceneSituation>();
            var battleSceneNode = titleTopNode.Connect<BattleSceneSituation>();
            var battlePauseNode = battleSceneNode.Connect<BattlePauseSituation>();
            _situationFlow.SetFallbackNode(modelViewerSceneNode);
            _situationFlow.SetFallbackNode(battleSceneNode);
        }
    }
}