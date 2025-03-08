using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Viewer関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// Viewer関連のSituationの初期化
        /// </summary>
        private void SetupViewerSituations(Situation parentSituation) {
            var modelViewerSituation = new ModelViewerSceneSituation();
            
            modelViewerSituation.SetParent(parentSituation);
        }
    }
}