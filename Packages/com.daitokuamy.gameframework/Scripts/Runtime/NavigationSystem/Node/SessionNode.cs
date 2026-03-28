namespace GameFramework.NavigationSystem {
    /// <summary>
    /// SessionとなるNavNode
    /// ※Root直下にのみ置けるNode
    /// </summary>
    public abstract class SessionNode : NavNode, ISessionNode {
        /// <inheritdoc/>
        protected override bool IsParallelLoading => false;
    }
}
