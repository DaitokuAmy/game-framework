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
        private void SetupIntroductionSituations(ISituationContainerProvider parentSituation, IScope scope) {
            var introductionSceneSituation = new IntroductionSceneSituation().ScopeTo(scope);
            introductionSceneSituation.SetParent(parentSituation);

            RegisterSituation(introductionSceneSituation);
        }
    }
}