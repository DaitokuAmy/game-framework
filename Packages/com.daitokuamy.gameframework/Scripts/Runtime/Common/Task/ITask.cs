namespace GameFramework {
    /// <summary>
    /// タスク用インターフェース
    /// </summary>
    public interface ITask {
        // タスクの有効状態
        bool IsActive { get; }

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();
    }
}