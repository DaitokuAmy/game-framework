namespace SampleGame.Lifecycle {
    /// <summary>
    /// SceneSessionNode基底
    /// </summary>
    public abstract class SceneSessionNode : GameFramework.NavigationSystems.SceneSessionNode {
        /// <inheritdoc/>
        protected override string EmptyScenePath => "Assets/SampleGame/Scenes/empty.unity";
    }
}