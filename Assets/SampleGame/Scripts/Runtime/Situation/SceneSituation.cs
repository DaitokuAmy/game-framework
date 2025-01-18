namespace SampleGame {
    /// <summary>
    /// SceneSituation基底
    /// </summary>
    public abstract class SceneSituation : GameFramework.SituationSystems.SceneSituation {
        protected override string EmptySceneAssetPath => "Assets/SampleGame/Scenes/empty.unity";
    }
}