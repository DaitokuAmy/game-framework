namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class OutGameMainSystemStarter : MainSystemStarter {
        /// <inheritdoc/>
        protected override int NodeId => AppNavigator.Id.SortieTop;
    }
}