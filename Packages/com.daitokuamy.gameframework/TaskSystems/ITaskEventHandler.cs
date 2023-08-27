namespace GameFramework.TaskSystems {
    /// <summary>
    /// タスクイベント通知用インターフェース
    /// </summary>
    public interface ITaskEventHandler {
        /// <summary>
        /// 登録時処理
        /// </summary>
        void OnRegistered(TaskRunner runner);

        /// <summary>
        /// 登録解除時処理
        /// </summary>
        void OnUnregistered(TaskRunner runner);
    }
}