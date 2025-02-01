namespace GameFramework.TaskSystems {
    /// <summary>
    /// Fixed更新可能なタスク用インターフェース
    /// </summary>
    public interface IFixedUpdatableTask : ITask {
        /// <summary>
        /// Fixed更新処理
        /// </summary>
        void FixedUpdate();
    }
}