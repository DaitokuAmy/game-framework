using System;

namespace GameFramework.NavigationSystems {
    /// <summary>
    /// NavNode遷移ルールを提供するインターフェース
    /// </summary>
    public interface INavNodeStateRouter : IStateRouter<Type, INavNode, NavNodeTree.TransitionOption> {
    }
}