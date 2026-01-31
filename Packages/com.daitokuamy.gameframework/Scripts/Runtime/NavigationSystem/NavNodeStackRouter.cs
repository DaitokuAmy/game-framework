namespace GameFramework.NavigationSystem {
    /// <summary>
    /// NavigationNode遷移に使うスタック
    /// </summary>
    public sealed class NavNodeStackRouter : StateStackRouter<int, INavNode, NavNodeTree.TransitionOption>, INavNodeStateRouter {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NavNodeStackRouter(NavNodeTree container)
            : base(container) {
        }
    }
}