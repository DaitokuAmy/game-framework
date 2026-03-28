using System;
using System.Collections.Generic;

namespace GameFramework.NavigationSystem {
    /// <summary>
    /// Navigationシステムを動かすためのエンジン
    /// </summary>
    public sealed class NavigationEngine : IDisposable {
        /// <summary>無効値を表すNodeId</summary>
        public const int InvalidNodeId = 0;

        private readonly NavNodeTree _tree;
        private readonly IStateRouter<int, INavNode, NavNodeTree.TransitionOption> _router;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal NavigationEngine(IRootNode rootNode, IReadOnlyDictionary<int, INavNode> nodeMap, Func<NavNodeTree, IStateRouter<int, INavNode, NavNodeTree.TransitionOption>> createRouterFunc) {
            _tree = new NavNodeTree(rootNode, nodeMap, this);
            _router = createRouterFunc?.Invoke(_tree);
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose() {
            _tree.Dispose();
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
        /// 指定した screen node へ遷移します。
        /// </summary>
        /// <param name="nodeId">遷移先NodeのId</param>
        /// <param name="option">遷移オプション</param>
        /// <param name="setupAction">遷移先の事前設定</param>
        /// <param name="transition">遷移方式</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<INavNode> TransitionTo<TScreen>(int nodeId, NavNodeTree.TransitionOption? option, Action<TScreen> setupAction, ITransition transition,
            params ITransitionEffect[] effects)
            where TScreen : class, IScreenNode {
            void Setup(INavNode node) {
                if (node is not TScreen screenNode) {
                    throw new InvalidOperationException($"Node <{nodeId}> is not {typeof(TScreen).Name}.");
                }

                setupAction?.Invoke(screenNode);
            }

            if (_router != null) {
                return _router.TransitionTo(nodeId, option ?? NavNodeTree.TransitionOption.Default, Setup, transition, effects);
            }

            return _tree.TransitionTo(nodeId, option ?? NavNodeTree.TransitionOption.Default, false, Setup, transition, effects);
        }

        /// <summary>
        /// 指定した screen node へ遷移します。
        /// </summary>
        /// <param name="nodeId">遷移先NodeのId</param>
        /// <param name="setupAction">遷移先の事前設定</param>
        /// <param name="transition">遷移方式</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<INavNode> TransitionTo<TScreen>(int nodeId, Action<TScreen> setupAction, ITransition transition, params ITransitionEffect[] effects)
            where TScreen : class, IScreenNode {
            return TransitionTo(nodeId, null, setupAction, transition, effects);
        }

        /// <summary>
        /// 指定した screen node へ遷移します。
        /// </summary>
        /// <param name="nodeId">遷移先NodeのId</param>
        /// <param name="transition">遷移方式</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<INavNode> TransitionTo<TScreen>(int nodeId, ITransition transition, params ITransitionEffect[] effects)
            where TScreen : class, IScreenNode {
            return TransitionTo<TScreen>(nodeId, null, null, transition, effects);
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        /// <param name="depth">戻る階層数(1以上)</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="setupAction">遷移先事前処理用関数</param>
        /// <param name="transition">遷移方式</param>
        /// <param name="effects">遷移演出</param>
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
        /// <param name="depth">戻る階層数(1以上)</param>
        /// <param name="setupAction">遷移先事前処理用関数</param>
        /// <param name="transition">遷移方式</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<INavNode> Back(int depth = 1, Action<INavNode> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects) {
            return Back(depth, null, setupAction, transition, effects);
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        /// <param name="setupAction">遷移先事前処理用関数</param>
        /// <param name="transition">遷移方式</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<INavNode> Back(Action<INavNode> setupAction, ITransition transition = null, params ITransitionEffect[] effects) {
            return Back(1, null, setupAction, transition, effects);
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        /// <param name="transition">遷移方式</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<INavNode> Back(ITransition transition = null, params ITransitionEffect[] effects) {
            return Back(1, null, null, transition, effects);
        }

        /// <summary>
        /// 状態リセット
        /// </summary>
        /// <param name="setupAction">遷移先事前処理用関数</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<INavNode> Reset(Action<INavNode> setupAction, params ITransitionEffect[] effects) {
            if (_router != null) {
                return _router.Reset(setupAction, effects);
            }

            return _tree.Reset(setupAction, effects);
        }

        /// <summary>
        /// 状態リセット
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle<INavNode> Reset(params ITransitionEffect[] effects) {
            return Reset(null, effects);
        }

        /// <summary>
        /// 現在遷移中なNodeの取得
        /// </summary>
        public INavNode GetCurrentNode() {
            return _tree.Current;
        }

        /// <summary>
        /// カレントNodeの親階層に指定のNavNode型が存在するかチェック
        /// </summary>
        public bool CheckNodeTypeInParent<TNode>()
            where TNode : INavNode {
            return _tree.CheckNodeTypeInParent<TNode>();
        }

        /// <summary>
        /// カレントNodeの親階層の中で指定の型のNodeを取得
        /// </summary>
        public TNode GetNodeInParent<TNode>()
            where TNode : INavNode {
            return _tree.GetNodeInParent<TNode>();
        }

        /// <summary>
        /// 指定Nodeの親階層の中で指定の型のNodeを取得
        /// </summary>
        public TNode GetNodeInParent<TNode>(int targetNodeId)
            where TNode : INavNode {
            return _tree.GetNodeInParent<TNode>(targetNodeId);
        }

        /// <summary>
        /// 戻り先のNodeの親階層の中で指定型のNodeを取得
        /// </summary>
        /// <param name="depth">戻る階層</param>
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
        /// 子孫階層の含まれている指定型NodeのNodeIdを取得
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
