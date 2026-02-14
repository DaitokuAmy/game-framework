using UnityEditor;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// EditorToolWindowで管理するモジュール基底
    /// </summary>
    public abstract class EditorToolModule<TWindow, TUserData>
        where TWindow : EditorToolWindow<TWindow, TUserData>
        where TUserData : class, new() {
        private DisposableScope _attachScope;
        private DisposableScope _startScope;

        /// <summary>表示名</summary>
        public abstract string DisplayName { get; }
        /// <summary>紐付け先Window</summary>
        protected TWindow Window { get; private set; }

        /// <summary>
        /// 紐付け処理
        /// </summary>
        internal void Attach(TWindow window) {
            if (_attachScope != null) {
                return;
            }

            Window = window;
            _attachScope = new DisposableScope();
            OnAttachInternal(_attachScope);
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        internal void Start() {
            if (_startScope != null) {
                return;
            }

            _startScope = new DisposableScope();
            OnStartInternal(_startScope);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        internal void Exit() {
            if (_startScope == null) {
                return;
            }

            var scope = _startScope;
            _startScope = null;
            OnExitInternal();
            scope.Dispose();
        }

        /// <summary>
        /// 紐付け解除処理
        /// </summary>
        internal void Detach() {
            Exit();

            if (_attachScope == null) {
                return;
            }

            var scope = _attachScope;
            _attachScope = null;
            OnDetachInternal();
            scope.Dispose();
            Window = null;
        }

        /// <summary>
        /// 紐付け時処理
        /// </summary>
        protected virtual void OnAttachInternal(IScope scope) { }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected virtual void OnStartInternal(IScope scope) { }

        /// <summary>
        /// 終了時処理
        /// </summary>
        protected virtual void OnExitInternal() { }

        /// <summary>
        /// 解除時処理
        /// </summary>
        protected virtual void OnDetachInternal() { }

        /// <summary>
        /// GUI描画処理
        /// </summary>
        public virtual void OnGUI() { }

        /// <summary>
        /// SceneGUI描画処理
        /// </summary>
        public virtual void OnSceneGUI(SceneView sceneView) { }

        /// <summary>
        /// 更新処理
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// 常時更新処理
        /// </summary>
        public virtual void OnEveryUpdate() { }
    }
}
