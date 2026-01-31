using System;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.NavigationSystems {
    /// <summary>
    /// Navigationシステムを動かすためのエンジン
    /// </summary>
    public sealed class NavigationEngine : IDisposable {
        /// <summary>無効扱いのNodeId</summary>
        public const int InvalidNodeId = 0;
        
        private readonly List<INavNode> _nodes = new();
        private readonly NavNodeTree _tree;
        private readonly IStateRouter<int, INavNode, NavNodeTree.TransitionOption> _router;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal NavigationEngine(IRootNode rootNode, IReadOnlyDictionary<int, INavNode> nodeMap, Func<NavNodeTree, IStateRouter<int, INavNode, NavNodeTree.TransitionOption>> createRouterFunc) {
            _nodes.AddRange(nodeMap.Values);
            _tree = new NavNodeTree(rootNode, nodeMap, this);
            _router = createRouterFunc.Invoke(_tree);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            // Treeの廃棄
            _tree.Dispose();

            // 各種NodeのRelease
            for (var i = _nodes.Count - 1; i >= 0; i--) {
                _nodes[i].Release();
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            _tree.Update();
        }

        /// <summary>
        /// Routerの取得
        /// </summary>
        public TRouter GetRouter<TRouter>()
            where TRouter : class, IStateRouter<int, INavNode, NavNodeTree.TransitionOption> {
            return _router as TRouter;
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="nodeId">遷移先NodeのId</param>
        /// <param name="option">遷移オプション</param>
        /// <param name="setupAction">遷移先Node</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> TransitionTo(int nodeId, NavNodeTree.TransitionOption? option, Action<IScreenNode> setupAction, ITransition transition, params ITransitionEffect[] effects) {
            if (_router != null) {
                return _router.TransitionTo(nodeId, option ?? NavNodeTree.TransitionOption.Default, node => {
                    setupAction?.Invoke((IScreenNode)node);
                }, transition, effects);
            }

            return _tree.TransitionTo(nodeId, option ?? NavNodeTree.TransitionOption.Default, false, node => {
                setupAction?.Invoke((IScreenNode)node);
            }, transition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="nodeId">遷移先NodeのId</param>
        /// <param name="setupAction">遷移先Node</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> TransitionTo(int nodeId, Action<IScreenNode> setupAction, ITransition transition, params ITransitionEffect[] effects) {
            return TransitionTo(nodeId, null, setupAction, transition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="nodeId">遷移先NodeのId</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> TransitionTo(int nodeId, ITransition transition, params ITransitionEffect[] effects) {
            return TransitionTo(nodeId, null, null, transition, effects);
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        /// <param name="depth">戻り階層数(1～)</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> Back(int depth = 1, NavNodeTree.TransitionOption? option = null, Action<INavNode> setupAction = null, ITransition transition = null,
            params ITransitionEffect[] effects) {
            if (_router != null) {
                return _router.Back(depth, option ?? NavNodeTree.TransitionOption.Default, setupAction, transition, effects);
            }

            throw new NotSupportedException("null router is not supported.");
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        /// <param name="depth">戻り階層数(1～)</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> Back(int depth = 1, Action<INavNode> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects) {
            return Back(depth, null, setupAction, transition, effects);
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> Back(Action<INavNode> setupAction, ITransition transition = null, params ITransitionEffect[] effects) {
            return Back(1, null, setupAction, transition, effects);
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> Back(ITransition transition = null, params ITransitionEffect[] effects) {
            return Back(1, null, null, transition, effects);
        }

        /// <summary>
        /// 状態リセット
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> Reset(Action<INavNode> setupAction, params ITransitionEffect[] effects) {
            if (_router != null) {
                return _router.Reset(setupAction, effects);
            }

            return _tree.Reset(setupAction, effects);
        }

        /// <summary>
        /// 状態リセット
        /// </summary>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<INavNode> Reset(params ITransitionEffect[] effects) {
            return Reset(null, effects);
        }

        /// <summary>
        /// 現在カレントなNodeの取得
        /// </summary>
        /// <returns></returns>
        public INavNode GetCurrentNode() {
            return _tree.Current;
        }

        /// <summary>
        /// カレントNodeの階層に特定のNavNode型が存在するかチェック
        /// ※カレントもチェック対象
        /// </summary>
        public bool CheckNodeTypeInParent<TNode>()
            where TNode : INavNode {
            return _tree.CheckNodeTypeInParent<TNode>();
        }

        /// <summary>
        /// カレントNodeの階層の中で特定の型のNodeを取得
        /// ※カレントもチェック対象
        /// </summary>
        public TNode GetNodeInParent<TNode>()
            where TNode : INavNode {
            return _tree.GetNodeInParent<TNode>();
        }

        /// <summary>
        /// 特定Nodeの階層の中で特定の型のNodeを取得
        /// ※指定Nodeもチェック対象
        /// </summary>
        public TNode GetNodeInParent<TNode>(int targetNodeId)
            where TNode : INavNode {
            return _tree.GetNodeInParent<TNode>(targetNodeId);
        }

        /// <summary>
        /// 戻り先のNodeの階層の中で特定型のNodeを取得
        /// ※戻り先もチェック対象
        /// </summary>
        /// <param name="depth">戻る深さ</param>
        public TNode GetBackNodeInParent<TNode>(int depth = 1)
            where TNode : INavNode {
            if (_router == null) {
                return default;
            }

            var backKey = _router.GetBackStateKey(depth);
            if (backKey == InvalidNodeId) {
                return default;
            }
            
            return _tree.GetNodeInParent<TNode>(backKey);
        }

        /// <summary>
        /// 子要素の含まれている該当Node型のNodeIdを検索
        /// </summary>
        public bool TryGetChildNodeId<TNode>(out int nodeId)
            where TNode : INavNode {
            nodeId = InvalidNodeId;
            
            return _tree.TryGetChildNodeId<TNode>(out nodeId);
        }

        /// <summary>
        /// NodeのPreLoad
        /// </summary>
        public AsyncOperationHandle PreLoad(int nodeId) {
            return _tree.PreLoad(nodeId);
        }
        
        /// <summary>
        /// NodeのPreLoadをUnload
        /// </summary>
        public void UnPreLoad(int nodeId) {
            _tree.UnPreLoad(nodeId);
        }
    }
}