namespace SampleGame.Lifecycle {
    /// <summary>
    /// SceneSituation基底
    /// </summary>
    public abstract class SceneSituation : GameFramework.SituationSystems.SceneSituation {
        /// <inheritdoc/>
        protected override string EmptySceneAssetPath => "Assets/SampleGame/Scenes/empty.unity";
    }
}