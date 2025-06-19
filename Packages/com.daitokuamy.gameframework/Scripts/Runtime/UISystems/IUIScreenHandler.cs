namespace GameFramework.UISystems
{
    /// <summary>
    /// UIScreenに紐づける処理を定義するインターフェース
    /// </summary>
    public interface IUIScreenHandler
    {
        /// <summary>アクティブ状態か</summary>
        bool IsActive { get; }
        
        /// <summary>
        /// 登録時処理
        /// </summary>
        void OnRegistered(UIScreen screen);

        /// <summary>
        /// 登録解除時処理
        /// </summary>
        void OnUnregistered();

        /// <summary>
        /// 開く前
        /// </summary>
        void PreOpen();

        /// <summary>
        /// 開いた後
        /// </summary>
        void PostOpen();

        /// <summary>
        /// アクティブ時
        /// </summary>
        void Activate();

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// 後更新処理
        /// </summary>
        void LateUpdate(float deltaTime);

        /// <summary>
        /// 非アクティブ時
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 閉じる前
        /// </summary>
        void PreClose();

        /// <summary>
        /// 閉じた後
        /// </summary>
        void PostClose();
    }
}