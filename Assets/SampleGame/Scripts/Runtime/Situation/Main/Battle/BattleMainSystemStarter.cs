namespace SampleGame.Battle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class BattleMainSystemStarter : MainSystemStarter {
        protected override ISituationSetup GetSituationSetup() {
            return new SituationSetup<BattleSceneSituation>();
        }
    }
}