namespace SampleGame.Lifecycle {
    /// <summary>
    /// ModelViewerを直接開始するためのStarter
    /// </summary>
    public class ModelViewerMainSystemStarter : MainSystemStarter {
        /// <inheritdoc/>
        protected override int NodeId => AppNavigator.Id.ModelViewer;
    }
}