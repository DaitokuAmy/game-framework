using GameFramework.SituationSystems;

namespace SampleGame {
    /// <summary>
    /// MainSystemのStarter
    /// </summary>
    public abstract class MainSystemStarter : GameFramework.Core.MainSystemStarter {
        /// <summary>
        /// MainSystem開始引数の取得
        /// </summary>
        public override object[] GetArguments() {
            return new object[] {
                GetStartSituation()
            };
        }

        /// <summary>
        /// 開始シチュエーションの取得
        /// </summary>
        protected abstract SceneSituation GetStartSituation();
    }
}