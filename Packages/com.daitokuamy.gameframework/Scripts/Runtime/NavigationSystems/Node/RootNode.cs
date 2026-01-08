namespace GameFramework.NavigationSystems {
    /// <summary>
    /// RootとなるNavNode
    /// </summary>
    public abstract class RootNode : NavNode, IRootNode {
        /// <inheritdoc/>
        protected override bool IsParallelLoading => false;
    }
}