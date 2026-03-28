namespace GameFramework.NavigationSystem {
    /// <summary>
    /// NavNode遷移ルールを提供するインターフェース
    /// </summary>
    public interface INavNodeStateRouter : IStateRouter<int, INavNode, NavNodeTree.TransitionOption> {
    }
}
