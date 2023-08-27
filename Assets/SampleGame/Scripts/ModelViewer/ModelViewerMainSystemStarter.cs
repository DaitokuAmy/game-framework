using GameFramework.SituationSystems;

namespace SampleGame {
    /// <summary>
    /// ModelViewerを直接開始するためのStarter
    /// </summary>
    public class ModelViewerMainSystemStarter : MainSystemStarter {
        /// <summary>
        /// 開始シチュエーションの取得
        /// </summary>
        protected override SceneSituation GetStartSituation() {
            // ModelViewerに遷移
            return new ModelViewerSceneSituation();
        }
    }
}