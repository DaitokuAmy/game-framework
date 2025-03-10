using GameFramework.SituationSystems;

namespace SampleGame.Lifecycle {
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
            var titleTopSituation = new TitleTopSituation();
            titleTopSituation.SetParent(introductionSceneSituation);
            var titleOptionSituation = new TitleOptionSituation();
            titleOptionSituation.SetParent(introductionSceneSituation);
        }
    }
}