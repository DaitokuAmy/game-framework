using GameFramework.SituationSystems;

namespace SampleGame {
    /// <summary>
    /// VfxViewerを直接開始するためのStarter
    /// </summary>
    public class VfxViewerMainSystemStarter : MainSystemStarter {
        /// <summary>
        /// 開始シチュエーションの取得
        /// </summary>
        protected override SceneSituation GetStartSituation() {
            // VfxViewerに遷移
            return new VfxViewerSceneSituation();
        }
    }
}