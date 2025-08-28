namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class OutGameMainSystemStarter : MainSystemStarter {
        protected override ISituationSetup GetSituationSetup() {
            return new SituationSetup<OutGameSceneSituation>();
        }
    }
}