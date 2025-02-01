namespace GameFramework.TaskSystems {
    /// <summary>
    /// 後更新可能なタスク用インターフェース
    /// </summary>
    public interface ILateUpdatableTask : ITask {
        /// <summary>
        /// 後更新処理
        /// </summary>
        void LateUpdate();
    }
}