using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Introduction;

namespace SampleGame {
    /// <summary>
    /// Introduction関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// Introduction関連のSituationの初期化
        /// </summary>
        private void SetupIntroductionSituations(Situation parentSituation) {
            var introductionSceneSituation = new IntroductionSceneSituation();
            introductionSceneSituation.SetParent(parentSituation);

            RegisterSituation(introductionSceneSituation);
        }
    }
}