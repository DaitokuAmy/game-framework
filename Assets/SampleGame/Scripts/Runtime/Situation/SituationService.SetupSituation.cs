using GameFramework.Core;
using SampleGame.Main;
using SampleGame.Introduction;
using SampleGame.ModelViewer;

namespace SampleGame {
    /// <summary>
    /// SituationService
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// Situationの初期化処理
        /// </summary>
        private void SetupSituations(IScope scope) {
            var mainSituation = new MainSituation();
            _situationContainer.Setup(mainSituation);
            
            SetupIntroductionSituations(mainSituation);
            SetupViewerSituations(mainSituation, scope);
        }

        /// <summary>
        /// 遷移処理の初期化
        /// </summary>
        private void SetupFlow(IScope scope) {
            var introductionNode = _situationFlow.ConnectRoot(GetSituation<IntroductionSceneSituation>());
            var modelViewerNode = introductionNode.Connect(GetSituation<ModelViewerSceneSituation>());
            _situationFlow.SetFallbackNode(modelViewerNode);
        }
    }
}