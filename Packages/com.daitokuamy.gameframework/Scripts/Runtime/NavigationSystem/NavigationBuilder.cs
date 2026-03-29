using System;
using System.Collections.Generic;

#if USE_VCONTAINER
using VContainer;
#endif

namespace GameFramework.NavigationSystem {
    /// <summary>
    /// Node構築用のインターフェース
    /// </summary>
    internal interface INavNodeBuilder {
        /// <summary>保持しているNavNode</summary>
        INavNode Node { get; }

        /// <summary>
        /// ビルド処理
        /// </summary>
        /// <param name="parentNode">登録親のNode</param>
        /// <param name="nodeMap">KeyValue登録用の辞書</param>
        void Build(INavNode parentNode, Dictionary<int, INavNode> nodeMap);
    }

    /// <summary>
    /// RootNode用のBuilder
    /// </summary>
    public sealed class RootNodeBuilder {
        private readonly int _nodeId;
        private readonly IRootNode _rootNode;
        private readonly List<INavNodeBuilder> _childBuilders = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal RootNodeBuilder(int nodeId, IRootNode rootNode, Action<RootNodeBuilder> buildAction = null) {
            _nodeId = nodeId;
            _rootNode = rootNode;
            buildAction?.Invoke(this);
        }

        /// <summary>
        /// SessionNodeの追加
        /// </summary>
        /// <param name="nodeId">登録するNode識別用Id</param>
        /// <param name="buildAction">子要素を追加するためのアクション</param>
        public RootNodeBuilder AddSession<TNode>(int nodeId, Action<SessionNodeBuilder> buildAction = null)
            where TNode : ISessionNode, new() {
            var child = new SessionNodeBuilder(nodeId, new TNode(), buildAction);
            _childBuilders.Add(child);
            return this;
        }

#if USE_VCONTAINER
        /// <summary>
        /// ビルド処理
        /// </summary>
        /// <param name="nodeMap">KeyValue登録用の辞書</param>
        /// <param name="parentObjectResolver">親として設定するVContainerのResolver</param>
        internal IRootNode Build(Dictionary<int, INavNode> nodeMap, IObjectResolver parentObjectResolver) {
#else
        /// <summary>
        /// ビルド処理
        /// </summary>
        /// <param name="nodeMap">KeyValue登録用の辞書</param>
        internal IRootNode Build(Dictionary<int, INavNode> nodeMap) {
#endif
            if (!nodeMap.TryAdd(_nodeId, _rootNode)) {
                throw new InvalidOperationException($"Node id <{_nodeId}:{_rootNode.GetType()}> is already registered.");
            }

#if USE_VCONTAINER
            _rootNode.Setup(_nodeId, null, parentObjectResolver);
#else
            _rootNode.Setup(_nodeId, null);
#endif
            foreach (var child in _childBuilders) {
                child.Build(_rootNode, nodeMap);
            }

            return _rootNode;
        }
    }

    /// <summary>
    /// SessionNode用のBuilder
    /// </summary>
    public sealed class SessionNodeBuilder : INavNodeBuilder {
        private readonly int _nodeId;
        private readonly ISessionNode _sessionNode;
        private readonly List<INavNodeBuilder> _childBuilders = new();

        /// <inheritdoc/>
        INavNode INavNodeBuilder.Node => _sessionNode;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal SessionNodeBuilder(int nodeId, ISessionNode sessionNode, Action<SessionNodeBuilder> buildAction = null) {
            _nodeId = nodeId;
            _sessionNode = sessionNode;
            buildAction?.Invoke(this);
        }

        /// <inheritdoc/>
        void INavNodeBuilder.Build(INavNode parentNode, Dictionary<int, INavNode> nodeMap) {
            if (!nodeMap.TryAdd(_nodeId, _sessionNode)) {
                throw new InvalidOperationException($"Node id <{_nodeId}:{_sessionNode.GetType()}> is already registered.");
            }

#if USE_VCONTAINER
            _sessionNode.Setup(_nodeId, parentNode, null);
#else
            _sessionNode.Setup(_nodeId, parentNode);
#endif
            foreach (var child in _childBuilders) {
                child.Build(_sessionNode, nodeMap);
            }
        }

        /// <summary>
        /// SessionNodeの追加
        /// </summary>
        /// <param name="nodeId">登録するNode識別用Id</param>
        /// <param name="buildAction">子要素を追加するためのアクション</param>
        public SessionNodeBuilder AddSession<TNode>(int nodeId, Action<SessionNodeBuilder> buildAction = null)
            where TNode : ISessionNode, new() {
            var child = new SessionNodeBuilder(nodeId, new TNode(), buildAction);
            _childBuilders.Add(child);
            return this;
        }

        /// <summary>
        /// ScreenNodeの追加
        /// </summary>
        /// <param name="nodeId">登録するNode識別用Id</param>
        /// <param name="buildAction">子要素を追加するためのアクション</param>
        public SessionNodeBuilder AddScreen<TNode>(int nodeId, Action<ScreenNodeBuilder> buildAction = null)
            where TNode : IScreenNode, new() {
            var child = new ScreenNodeBuilder(nodeId, new TNode(), buildAction);
            _childBuilders.Add(child);
            return this;
        }
    }

    /// <summary>
    /// ScreenNode用のBuilder
    /// </summary>
    public sealed class ScreenNodeBuilder : INavNodeBuilder {
        private readonly int _nodeId;
        private readonly IScreenNode _screenNode;
        private readonly List<INavNodeBuilder> _childBuilders = new();

        /// <inheritdoc/>
        INavNode INavNodeBuilder.Node => _screenNode;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal ScreenNodeBuilder(int nodeId, IScreenNode screenNode, Action<ScreenNodeBuilder> buildAction = null) {
            _nodeId = nodeId;
            _screenNode = screenNode;
            buildAction?.Invoke(this);
        }

        /// <inheritdoc/>
        void INavNodeBuilder.Build(INavNode parentNode, Dictionary<int, INavNode> nodeMap) {
            if (!nodeMap.TryAdd(_nodeId, _screenNode)) {
                throw new InvalidOperationException($"Node id <{_nodeId}:{_screenNode.GetType()}> is already registered.");
            }

#if USE_VCONTAINER
            _screenNode.Setup(_nodeId, parentNode, null);
#else
            _screenNode.Setup(_nodeId, parentNode);

#endif
            foreach (var child in _childBuilders) {
                child.Build(_screenNode, nodeMap);
            }
        }

        /// <summary>
        /// ScreenNodeの追加
        /// </summary>
        /// <param name="nodeId">登録するNode識別用Id</param>
        /// <param name="buildAction">子要素を追加するためのアクション</param>
        public ScreenNodeBuilder AddScreen<TNode>(int nodeId, Action<ScreenNodeBuilder> buildAction = null)
            where TNode : IScreenNode, new() {
            var child = new ScreenNodeBuilder(nodeId, new TNode(), buildAction);
            _childBuilders.Add(child);
            return this;
        }
    }

    /// <summary>
    /// NavigationEngineのBuilder
    /// </summary>
    public sealed class NavigationEngineBuilder {
        private readonly Dictionary<int, INavNode> _nodeMap = new();

        private RootNodeBuilder _rootNodeBuilder;
        private Func<NavNodeTree, INavNodeStateRouter> _createRouterFunc;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private NavigationEngineBuilder() {
        }

        /// <summary>
        /// ビルダーの生成
        /// </summary>
        public static NavigationEngineBuilder Create() {
            return new NavigationEngineBuilder();
        }

        /// <summary>
        /// ライフサイクルを表すツリー構造の生成
        /// </summary>
        /// <param name="nodeId">登録するNode識別用Id</param>
        /// <param name="buildAction">子要素を登録するためのアクション</param>
        public NavigationEngineBuilder CreateLifecycle<TNode>(int nodeId, Action<RootNodeBuilder> buildAction)
            where TNode : IRootNode, new() {
            if (_rootNodeBuilder != null) {
                throw new InvalidOperationException("RootNode is already set.");
            }

            _rootNodeBuilder = new RootNodeBuilder(nodeId, new TNode(), buildAction);
            return this;
        }

        /// <summary>
        /// Node遷移用のルーター設定
        /// </summary>
        /// <param name="createFunc">Routerの生成処理</param>
        public NavigationEngineBuilder CreateRouter(Func<NavNodeTree, INavNodeStateRouter> createFunc) {
            _createRouterFunc = createFunc;
            return this;
        }

#if USE_VCONTAINER
        /// <summary>
        /// エンジンのビルド
        /// </summary>
        /// <param name="parentObjectResolver">VContainer用の親となるResolver</param>
        public NavigationEngine Build(IObjectResolver parentObjectResolver = null) {
#else
        /// <summary>
        /// エンジンのビルド
        /// </summary>
        public NavigationEngine Build() {
#endif
            if (_rootNodeBuilder == null) {
                throw new InvalidOperationException("RootNode is not set. Call CreateLifecycle(...) before Build().");
            }

#if USE_VCONTAINER
            var rootNode = _rootNodeBuilder.Build(_nodeMap, parentObjectResolver);
#else
            var rootNode = _rootNodeBuilder.Build(_nodeMap);
#endif
            var engine = new NavigationEngine(rootNode, _nodeMap, _createRouterFunc);
            return engine;
        }
    }
}
