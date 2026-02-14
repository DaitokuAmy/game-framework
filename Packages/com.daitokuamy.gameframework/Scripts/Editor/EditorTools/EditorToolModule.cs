using UnityEditor;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// EditorToolWindowで管理されるモジュール基底
    /// </summary>
    public abstract class EditorToolModule<TWindow, TConfigData>
        where TWindow : EditorToolWindow<TWindow, TConfigData>
        where TConfigData : class, new() {
        private DisposableScope _attachScope;
        private DisposableScope _startScope;

        /// <summary>表示名</summary>
        public abstract string DisplayName { get; }

        /// <summary>紐付け先Window</summary>
        protected TWindow Window { get; private set; }

        /// <summary>
        /// Windowへアタッチ
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
        /// モジュール開始
        /// </summary>
        internal void Start() {
            if (_startScope != null) {
                return;
            }

            _startScope = new DisposableScope();
            OnStartInternal(_startScope);
        }

        /// <summary>
        /// モジュール終了
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
        /// Windowからデタッチ
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
        /// Attach時処理
        /// </summary>
        protected virtual void OnAttachInternal(IScope scope) { }

        /// <summary>
        /// Start時処理
        /// </summary>
        protected virtual void OnStartInternal(IScope scope) { }

        /// <summary>
        /// Exit時処理
        /// </summary>
        protected virtual void OnExitInternal() { }

        /// <summary>
        /// Detach時処理
        /// </summary>
        protected virtual void OnDetachInternal() { }

        /// <summary>
        /// GUI描画
        /// </summary>
        public virtual void OnGUI() { }

        /// <summary>
        /// SceneGUI描画
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
