using GameFramework.SituationSystems;

namespace SampleGame {
    /// <summary>
    /// Fieldを直接開始するためのStarter
    /// </summary>
    public class FieldMainSystemStarter : MainSystemStarter {
        /// <summary>
        /// 開始シチュエーションの取得
        /// </summary>
        protected override SceneSituation GetStartSituation() {
            // Login > Fieldに遷移
            return new LoginSceneSituation(new FieldSceneSituation());
        }
    }
}