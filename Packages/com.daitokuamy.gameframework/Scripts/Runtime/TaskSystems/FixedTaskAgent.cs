using System;

namespace GameFramework.TaskSystems {
    /// <summary>
    /// 固定更新用タスク用インターフェース
    /// </summary>
    public class FixedTaskAgent : TaskAgent, IFixedUpdatableTask {
        /// <summary>固定更新通知</summary>
        public event Action FixedUpdateEvent;

        /// <summary>
        /// 固定更新
        /// </summary>
        void IFixedUpdatableTask.FixedUpdate() {
            FixedUpdateEvent?.Invoke();
        }
    }
}