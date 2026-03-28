using System.Collections;
using System.Collections.Generic;

#if USE_VCONTAINER
using VContainer;
#endif

namespace GameFramework.NavigationSystem {
    /// <summary>
    /// NavNode基底
    /// </summary>
    public abstract class NavNode : INavNode {
        private readonly List<INavNode> _children = new();

        private DisposableScope _standbyScope;
        private DisposableScope _loadScope;
        private DisposableScope _initializeScope;
        private DisposableScope _activateScope;
        private int _nodeId;
        private INavNode _parent;

        /// <inheritdoc/>
        bool INavNode.IsParallelLoading => IsParallelLoading;
        /// <inheritdoc/>
        int INavNode.NodeId => _nodeId;
        /// <inheritdoc/>
        INavNode INavNode.Parent => _parent;
        /// <inheritdoc/>
        IReadOnlyList<INavNode> INavNode.Children => _children;
        /// <inheritdoc/>
        bool INavNode.IsActive => _activateScope != null;

        /// <summary>Loadを並列で実行可能か</summary>
        protected virtual bool IsParallelLoading => true;

#if USE_VCONTAINER
        /// <summary>VContainer用のResolver</summary>
        public IObjectResolver ObjectResolver { get; private set; }
#endif

        /// <summary>Navigation制御用エンジン</summary>
        protected NavigationEngine Engine { get; private set; }

        /// <inheritdoc/>
        void INavNode.SetFocus(bool focus) {
            SetFocus(focus);
        }

        /// <inheritdoc/>
        ITransition INavNode.OverrideTransition(INavNode nextNode, ITransition transition) {
            return OverrideTransition(nextNode, transition);
        }

#if USE_VCONTAINER
        /// <inheritdoc/>
        void INavNode.Setup(int nodeId, INavNode parent, IObjectResolver parentObjectResolver) {
#else
        /// <inheritdoc/>
        void INavNode.Setup(int nodeId, INavNode parent) {
#endif
            _nodeId = nodeId;

            if (_parent is NavNode prevParentNode) {
                prevParentNode._children.Remove(this);
            }

            _parent = parent;
            if (_parent is NavNode parentNode) {
                parentNode._children.Add(this);
            }

#if USE_VCONTAINER
            if (parentObjectResolver != null) {
                ObjectResolver = parentObjectResolver.CreateScope(Configure);
            }
            else {
                var builder = new ContainerBuilder();
                Configure(builder);
                ObjectResolver = builder.Build();
            }
#endif
        }

        /// <inheritdoc/>
        void INavNode.Standby(NavigationEngine engine) {
            _standbyScope = new DisposableScope();
            Engine = engine;
#if USE_VCONTAINER
            ObjectResolver.Inject(this);
#endif
            Standby(_standbyScope);
        }

        /// <inheritdoc/>
        IEnumerator INavNode.LoadRoutine(TransitionHandle<INavNode> handle) {
            _loadScope = new DisposableScope();
            yield return LoadRoutine(handle, _loadScope);
        }

        /// <inheritdoc/>
        IEnumerator INavNode.InitializeRoutine(TransitionHandle<INavNode> handle) {
            _initializeScope = new DisposableScope();
            yield return InitializeRoutine(handle, _initializeScope);
        }

        /// <inheritdoc/>
        void INavNode.Activate(TransitionHandle<INavNode> handle) {
            _activateScope = new DisposableScope();
            Activate(handle, _activateScope);
        }

        /// <inheritdoc/>
        void INavNode.UpdateAlways() {
            UpdateAlways();
        }

        /// <inheritdoc/>
        void INavNode.Update() {
            Update();
        }

        /// <inheritdoc/>
        void INavNode.Deactivate(TransitionHandle<INavNode> handle) {
            Deactivate(handle);
            _activateScope?.Dispose();
            _activateScope = null;
        }

        /// <inheritdoc/>
        void INavNode.Terminate(TransitionHandle<INavNode> handle) {
            Terminate(handle);
            _initializeScope?.Dispose();
            _initializeScope = null;
        }

        /// <inheritdoc/>
        void INavNode.Unload(TransitionHandle<INavNode> handle) {
            Unload(handle);
            _loadScope?.Dispose();
            _loadScope = null;
        }

        /// <inheritdoc/>
        void INavNode.Release() {
            if (_standbyScope == null && Engine == null
#if USE_VCONTAINER
                && ObjectResolver == null
#endif
               ) {
                return;
            }

            Release();
#if USE_VCONTAINER
            ObjectResolver?.Dispose();
            ObjectResolver = null;
#endif
            Engine = null;
            _standbyScope?.Dispose();
            _standbyScope = null;
        }

        /// <inheritdoc/>
        void INavNode.Shutdown(TransitionHandle<INavNode> handle) {
            Shutdown(handle);

            if (_activateScope != null) {
                ((INavNode)this).Deactivate(handle);
            }

            if (_initializeScope != null) {
                ((INavNode)this).Terminate(handle);
            }

            if (_loadScope != null) {
                ((INavNode)this).Unload(handle);
            }

            if (_standbyScope != null) {
                ((INavNode)this).Release();
            }
        }

        /// <summary>
        /// フォーカス状態の設定
        /// </summary>
        /// <param name="focus">フォーカス状態</param>
        protected virtual void SetFocus(bool focus) { }

        /// <summary>
        /// 遷移方法を上書きする場合の処理
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="transition">現在の遷移方式</param>
        protected virtual ITransition OverrideTransition(INavNode nextNode, ITransition transition) {
            return transition;
        }

        /// <summary>
        /// スタンバイ処理
        /// </summary>
        protected virtual void Standby(IScope scope) { }

#if USE_VCONTAINER
        /// <summary>
        /// DIコンテナの初期化
        /// </summary>
        protected virtual void Configure(IContainerBuilder builder) { }
#endif

        /// <summary>
        /// 読み込み処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">読み込みスコープ</param>
        protected virtual IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield break;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">初期化スコープ</param>
        protected virtual IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield break;
        }

        /// <summary>
        /// アクティブ化処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">アクティブスコープ</param>
        protected virtual void Activate(TransitionHandle<INavNode> handle, IScope scope) { }

        /// <summary>
        /// アクティブ状態に関係なく常時呼ばれる更新
        /// </summary>
        protected virtual void UpdateAlways() { }

        /// <summary>
        /// アクティブ状態のときだけ呼ばれる更新
        /// </summary>
        protected virtual void Update() { }

        /// <summary>
        /// 非アクティブ化処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void Deactivate(TransitionHandle<INavNode> handle) { }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void Terminate(TransitionHandle<INavNode> handle) { }

        /// <summary>
        /// アンロード処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void Unload(TransitionHandle<INavNode> handle) { }

        /// <summary>
        /// 解放処理
        /// </summary>
        protected virtual void Release() { }

        /// <summary>
        /// 強制終了処理
        /// </summary>
        protected virtual void Shutdown(TransitionHandle<INavNode> handle) { }
    }
}
