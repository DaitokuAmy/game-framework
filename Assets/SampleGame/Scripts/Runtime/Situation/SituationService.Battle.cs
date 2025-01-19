using GameFramework.SituationSystems;
using SampleGame.Battle;

namespace SampleGame {
    /// <summary>
    /// Battle関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <summary>
        /// Battle関連のSituationの初期化
        /// </summary>
        private void SetupBattleSituations(Situation parentSituation) {
            var battleSceneSituation = new BattleSceneSituation();
            battleSceneSituation.SetParent(parentSituation);
            var battlePauseSituation = new BattlePauseSituation();
            battlePauseSituation.SetParent(battleSceneSituation);
        }
    }
}