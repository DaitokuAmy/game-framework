using GameFramework.SituationSystems;

namespace SampleGame {
    /// <summary>
    /// Battleを直接開始するためのStarter
    /// </summary>
    public class BattleMainSystemStarter : MainSystemStarter {
        /// <summary>
        /// 開始シチュエーションの取得
        /// </summary>
        protected override SceneSituation GetStartSituation() {
            // Login > Battleに遷移
            return new LoginSceneSituation(new BattleSceneSituation());
        }
    }
}