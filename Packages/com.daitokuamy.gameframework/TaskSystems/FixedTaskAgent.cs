using System;

namespace GameFramework.TaskSystems {
    /// <summary>
    /// 固定更新用タスク用インターフェース
    /// </summary>
    public class FixedTaskAgent : TaskAgent, IFixedUpdatableTask {
        // 固定更新通知
        public event Action OnFixedUpdate;

        /// <summary>
        /// 固定更新
        /// </summary>
        void IFixedUpdatableTask.FixedUpdate() {
            OnFixedUpdate?.Invoke();
        }
    }
}