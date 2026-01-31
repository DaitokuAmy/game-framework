namespace GameFramework.NavigationSystems {
    /// <summary>
    /// NavNode遷移に使うツリー
    /// </summary>
    public sealed class NavNodeTreeRouter : StateTreeRouter<int, INavNode, NavNodeTree.TransitionOption>, INavNodeStateRouter {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NavNodeTreeRouter(NavNodeTree container)
            : base(container) {
        }
    }
}