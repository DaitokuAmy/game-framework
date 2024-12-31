using GameFramework.Core;
using SampleGame.Main;

namespace SampleGame {
    /// <summary>
    /// SituationService
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// Situationの初期化処理
        /// </summary>
        private void SetupSituations(IScope scope) {
            var mainSituation = new MainSituation().ScopeTo(scope);
            mainSituation.SetParent(_situationRunner);
            
            //SetupIntroductionSituations(mainSituation, scope);
        }

        /// <summary>
        /// 遷移処理の初期化
        /// </summary>
        private void SetupFlow(IScope scope) {
            //var introductionNode = _situationFlow.ConnectRoot(GetSituation<IntroductionSceneSituation>());
        }
    }
}