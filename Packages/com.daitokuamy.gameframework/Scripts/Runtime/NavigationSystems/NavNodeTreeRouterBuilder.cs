using System;
using System.Collections.Generic;

namespace GameFramework.NavigationSystems {
    /// <summary>
    /// StateTreeRouter用のBuilder
    /// </summary>
    public sealed class NavNodeTreeNodeBuilder {
        /// <summary>
        /// Shortcut情報
        /// </summary>
        private class ShortcutInfo {
            public Type BaseNodeKey;
            public NavNodeTreeNodeBuilder BaseNodeBuilder;
        }

        private readonly NavNodeTreeNodeBuilder _parent;
        private readonly Type _key;
        private readonly List<NavNodeTreeNodeBuilder> _children = new();
        private readonly List<ShortcutInfo> _shortcutInfos = new();

        private StateTreeNode<Type> _node;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal NavNodeTreeNodeBuilder(Type key, NavNodeTreeNodeBuilder parent, Action<NavNodeTreeNodeBuilder> buildAction) {
            _key = key;
            _parent = parent;
            buildAction?.Invoke(this);
        }

        /// <summary>
        /// 遷移先の接続
        /// </summary>
        /// <param name="buildAction">ネスト時に利用するアクション</param>
        public NavNodeTreeNodeBuilder Connect<TNodeType>(Action<NavNodeTreeNodeBuilder> buildAction = null)
            where TNodeType : IScreenNode {
            var child = new NavNodeTreeNodeBuilder(typeof(TNodeType), this, buildAction);
            _children.Add(child);
            return this;
        }

        /// <summary>
        /// Shortcutの指定
        /// </summary>
        /// <param name="baseNodeKey">スコープを表すNodeKey</param>
        public NavNodeTreeNodeBuilder SetShortcutScope(Type baseNodeKey) {
            _shortcutInfos.Add(new ShortcutInfo { BaseNodeKey = baseNodeKey });
            return this;
        }

        /// <summary>
        /// Shortcutの指定
        /// </summary>
        /// <param name="baseNodeBuilder">スコープを表すNodeBuilder</param>
        public NavNodeTreeNodeBuilder SetShortcutScope(NavNodeTreeNodeBuilder baseNodeBuilder) {
            _shortcutInfos.Add(new ShortcutInfo { BaseNodeBuilder = baseNodeBuilder });
            return this;
        }

        /// <summary>
        /// GlobalShortcutの指定
        /// </summary>
        public NavNodeTreeNodeBuilder SetGlobalShortcut() {
            _shortcutInfos.Add(new ShortcutInfo { BaseNodeKey = null });
            return this;
        }

        /// <summary>
        /// 構築処理
        /// </summary>
        internal void Build(NavNodeTreeRouter router) {
            _node = router.ConnectRoot(_key);

            foreach (var info in _shortcutInfos) {
                var baseNode = info.BaseNodeBuilder?._node ?? FindNodeInParent(info.BaseNodeKey);
                router.SetShortcutNode(_node, baseNode);
            }

            foreach (var child in _children) {
                child.Build(router, _node);
            }
        }

        /// <summary>
        /// 構築処理
        /// </summary>
        internal void Build(NavNodeTreeRouter router, StateTreeNode<Type> parent) {
            _node = parent.Connect(_key);

            foreach (var info in _shortcutInfos) {
                var baseNode = info.BaseNodeBuilder?._node ?? FindNodeInParent(info.BaseNodeKey);
                router.SetShortcutNode(_node, baseNode);
            }

            foreach (var child in _children) {
                child.Build(router, _node);
            }
        }

        /// <summary>
        /// 親要素の生成済みNodeを再帰的に探す
        /// </summary>
        private StateTreeNode<Type> FindNodeInParent(Type key) {
            if (key == null) {
                return null;
            }

            var p = _parent;
            while (p != null) {
                if (p._key == key) {
                    return p._node;
                }

                p = p._parent;
            }

            throw new KeyNotFoundException($"Not found base node key:{key}");
        }
    }

    /// <summary>
    /// StateTreeRouterを構築するためのBuilder
    /// </summary>
    public sealed class NavNodeTreeRouterBuilder {
        private readonly List<NavNodeTreeNodeBuilder> _rootBuilders = new();

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
        public NavNodeTreeRouterBuilder AddRoot<TNodeType>(Action<NavNodeTreeNodeBuilder> buildAction = null)
            where TNodeType : IScreenNode {
            var builder = new NavNodeTreeNodeBuilder(typeof(TNodeType), null, buildAction);
            _rootBuilders.Add(builder);
            return this;
        }

        /// <summary>
        /// 構築処理
        /// </summary>
        public NavNodeTreeRouter Build(NavNodeTreeRouter router) {
            foreach (var rootBuilder in _rootBuilders) {
                rootBuilder.Build(router);
            }

            return router;
        }
    }
}