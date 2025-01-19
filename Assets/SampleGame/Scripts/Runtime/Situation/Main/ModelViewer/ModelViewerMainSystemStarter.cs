namespace SampleGame.ModelViewer {
    /// <summary>
    /// ModelViewerを直接開始するためのStarter
    /// </summary>
    public class ModelViewerMainSystemStarter : MainSystemStarter {
        protected override ISituationSetup GetSituationSetup() {
            return new SituationSetup<ModelViewerSceneSituation>();
        }
    }
}