using System;
using System.Collections.Generic;

namespace GameFramework.NavigationSystem {
    /// <summary>
    /// NavNodeTreeRouter用のBuilder
    /// </summary>
    public sealed class NavNodeTreeRouterNodeBuilder {
        /// <summary>
        /// Shortcut情報
        /// </summary>
        private class ShortcutInfo {
            public int? BaseNodeId;
            public NavNodeTreeRouterNodeBuilder BaseNodeBuilder;
        }

        private readonly NavNodeTreeRouterNodeBuilder _parent;
        private readonly int _nodeId;
        private readonly List<NavNodeTreeRouterNodeBuilder> _children = new();
        private readonly List<ShortcutInfo> _shortcutInfos = new();

        private StateTreeNode<int> _node;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal NavNodeTreeRouterNodeBuilder(int nodeId, NavNodeTreeRouterNodeBuilder parent, Action<NavNodeTreeRouterNodeBuilder> buildAction) {
            _nodeId = nodeId;
            _parent = parent;
            buildAction?.Invoke(this);
        }

        /// <summary>
        /// 遷移先の接続
        /// </summary>
        /// <param name="nodeId">接続するNodeId</param>
        /// <param name="buildAction">ネスト時に利用するアクション</param>
        public NavNodeTreeRouterNodeBuilder Connect(int nodeId, Action<NavNodeTreeRouterNodeBuilder> buildAction = null) {
            var child = new NavNodeTreeRouterNodeBuilder(nodeId, this, buildAction);
            _children.Add(child);
            return this;
        }

        /// <summary>
        /// Shortcutの指定
        /// </summary>
        /// <param name="baseNodeId">スコープを表すNodeId（該当Node以下）</param>
        public NavNodeTreeRouterNodeBuilder SetShortcutScope(int baseNodeId) {
            _shortcutInfos.Add(new ShortcutInfo { BaseNodeId = baseNodeId });
            return this;
        }

        /// <summary>
        /// Shortcutの指定
        /// </summary>
        /// <param name="baseNodeBuilder">スコープを表すNodeBuilder</param>
        public NavNodeTreeRouterNodeBuilder SetShortcutScope(NavNodeTreeRouterNodeBuilder baseNodeBuilder) {
            _shortcutInfos.Add(new ShortcutInfo { BaseNodeBuilder = baseNodeBuilder });
            return this;
        }

        /// <summary>
        /// GlobalShortcutの指定
        /// </summary>
        public NavNodeTreeRouterNodeBuilder SetGlobalShortcut() {
            _shortcutInfos.Add(new ShortcutInfo { BaseNodeId = null });
            return this;
        }

        /// <summary>
        /// 構築処理
        /// </summary>
        internal void Build(NavNodeTreeRouter router) {
            _node = router.ConnectRoot(_nodeId);

            foreach (var info in _shortcutInfos) {
                var baseNode = info.BaseNodeBuilder?._node ?? FindNodeInParent(info.BaseNodeId);
                router.SetShortcutNode(_node, baseNode);
            }

            foreach (var child in _children) {
                child.Build(router, _node);
            }
        }

        /// <summary>
        /// 構築処理
        /// </summary>
        internal void Build(NavNodeTreeRouter router, StateTreeNode<int> parent) {
            _node = parent.Connect(_nodeId);

            foreach (var info in _shortcutInfos) {
                var baseNode = info.BaseNodeBuilder?._node ?? FindNodeInParent(info.BaseNodeId);
                router.SetShortcutNode(_node, baseNode);
            }

            foreach (var child in _children) {
                child.Build(router, _node);
            }
        }

        /// <summary>
        /// 親要素の生成済みNodeを再帰的に探す
        /// </summary>
        private StateTreeNode<int> FindNodeInParent(int? nodeId) {
            if (nodeId == null) {
                return null;
            }

            var p = _parent;
            while (p != null) {
                if (p._nodeId == nodeId) {
                    return p._node;
                }

                p = p._parent;
            }

            throw new KeyNotFoundException($"Not found base node key:{nodeId}");
        }
    }

    /// <summary>
    /// StateTreeRouterを構築するためのBuilder
    /// </summary>
    public sealed class NavNodeTreeRouterBuilder {
        private readonly List<NavNodeTreeRouterNodeBuilder> _rootBuilders = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private NavNodeTreeRouterBuilder() {
        }

        /// <summary>
        /// Builderの生成
        /// </summary>
        public static NavNodeTreeRouterBuilder Create() {
            return new NavNodeTreeRouterBuilder();
        }

        /// <summary>
        /// ルートの追加
        /// </summary>
        public NavNodeTreeRouterBuilder AddRoot(int nodeId, Action<NavNodeTreeRouterNodeBuilder> buildAction = null) {
            var builder = new NavNodeTreeRouterNodeBuilder(nodeId, null, buildAction);
            _rootBuilders.Add(builder);
            return this;
        }

        /// <summary>
        /// 構築処理
        /// </summary>
        public NavNodeTreeRouter Build(NavNodeTree lifecycle) {
            var router = new NavNodeTreeRouter(lifecycle);
            foreach (var rootBuilder in _rootBuilders) {
                rootBuilder.Build(router);
            }

            return router;
        }
    }
}