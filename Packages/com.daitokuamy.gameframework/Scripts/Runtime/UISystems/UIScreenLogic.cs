using GameFramework.LogicSystems;

namespace GameFramework.UISystems
{
    /// <summary>
    /// UIScreenに紐づけるロジック
    /// </summary>
    public class UIScreenLogic<T> : Logic, IUIScreenHandler
        where T : UIScreen
    {        
        /// <summary>制御対象のスクリーン</summary>
        protected T Screen { get; private set; }

        /// <summary>
        /// 登録時処理
        /// </summary>
        void IUIScreenHandler.OnRegistered(UIScreen screen)
        {
            Screen = screen as T;
        }

        /// <summary>
        /// 登録解除時処理
        /// </summary>
        void IUIScreenHandler.OnUnregistered()
        {
            Screen = null;
        }

        /// <summary>
        /// 開く前
        /// </summary>
        void IUIScreenHandler.PreOpen()
        {
            if (IsDisposed)
            {
                return;
            }

            PreOpenInternal();
        }

        /// <summary>
        /// 開いた後
        /// </summary>
        void IUIScreenHandler.PostOpen()
        {
            if (IsDisposed)
            {
                return;
            }

            PostOpenInternal();
        }

        /// <summary>
        /// アクティブ時
        /// </summary>
        void IUIScreenHandler.Activate()
        {
            Activate();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void IUIScreenHandler.Update(float deltaTime)
        {
            // 使わない
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        void IUIScreenHandler.LateUpdate(float deltaTime)
        {
            // 使わない
        }

        /// <summary>
        /// 非アクティブ時
        /// </summary>
        void IUIScreenHandler.Deactivate()
        {
            Deactivate();
        }

        /// <summary>
        /// 閉じる前
        /// </summary>
        void IUIScreenHandler.PreClose()
        {
            if (IsDisposed)
            {
                return;
            }

            PreCloseInternal();
        }

        /// <summary>
        /// 閉じた後
        /// </summary>
        void IUIScreenHandler.PostClose()
        {
            if (IsDisposed)
            {
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