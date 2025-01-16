namespace SampleGame.Introduction {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class IntroductionMainSystemStarter : MainSystemStarter {
        protected override ISituationSetup GetSituationSetup() {
            return new SituationSetup<IntroductionSceneSituation>();
        }
    }
}