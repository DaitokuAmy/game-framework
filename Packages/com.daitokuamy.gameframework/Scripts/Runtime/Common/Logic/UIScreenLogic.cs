using GameFramework.UISystem;

namespace GameFramework {
    /// <summary>
    /// UIScreenに紐づけるロジック
    /// </summary>
    public class UIScreenLogic<TScreen> : UpdatableLogic, IUIScreenHandler
        where TScreen : UIScreen {
        /// <summary>制御対象のスクリーン</summary>
        protected TScreen Screen { get; private set; }

        /// <inheritdoc/>
        void IUIScreenHandler.OnRegistered(UIScreen screen) {
            Screen = screen as TScreen;
        }

        /// <inheritdoc/>
        void IUIScreenHandler.OnUnregistered() {
            Screen = null;
        }

        /// <inheritdoc/>
        void IUIScreenHandler.PreOpen() {
            if (IsDisposed) {
                return;
            }

            PreOpenInternal();
        }

        /// <inheritdoc/>
        void IUIScreenHandler.PostOpen() {
            if (IsDisposed) {
                return;
            }

            PostOpenInternal();
        }

        /// <inheritdoc/>
        void IUIScreenHandler.Activate() {
            Activate();
        }

        /// <inheritdoc/>
        void IUIScreenHandler.Update(float deltaTime) {
            // 使わない
        }

        /// <inheritdoc/>
        void IUIScreenHandler.LateUpdate(float deltaTime) {
            // 使わない
        }

        /// <inheritdoc/>
        void IUIScreenHandler.Deactivate() {
            Deactivate();
        }

        /// <inheritdoc/>
        void IUIScreenHandler.PreClose() {
            if (IsDisposed) {
                return;
            }

            PreCloseInternal();
        }

        /// <inheritdoc/>
        void IUIScreenHandler.PostClose() {
            if (IsDisposed) {
                return;
            }

            PostCloseInternal();
        }

        /// <summary>
        /// 開く前の処理
        /// </summary>
        protected virtual void PreOpenInternal() { }

        /// <summary>
        /// 開いた後の処理
        /// </summary>
        protected virtual void PostOpenInternal() { }

        /// <summary>
        /// 閉じる前の処理
        /// </summary>
        protected virtual void PreCloseInternal() { }

        /// <summary>
        /// 閉じた後の処理
        /// </summary>
        protected virtual void PostCloseInternal() { }
    }
}
