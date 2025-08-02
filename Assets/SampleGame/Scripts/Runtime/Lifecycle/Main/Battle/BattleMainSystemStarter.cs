using GameFramework;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class BattleMainSystemStarter : MainSystemStarter {
        private enum Hoge {
            X,
            Y,
            Z,
        }
        
        protected override ISituationSetup GetSituationSetup() {
            return new SituationSetup<BattleSceneSituation>();
        }
    }
}