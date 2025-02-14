using System;

namespace GameFramework.TaskSystems {
    /// <summary>
    /// タスク用インターフェース
    /// </summary>
    public class TaskAgent : DisposableLateUpdatableTask {
        // 更新通知
        public event Action OnUpdate;
        // 後更新通知
        public event Action OnLateUpdate;

        // Taskが有効か
        public new bool IsActive { get; set; } = true;

        /// <summary>
        /// タスク更新
        /// </summary>
        protected override void UpdateInternal() {
            OnUpdate?.Invoke();
        }

        /// <summary>
        /// タスク後更新
        /// </summary>
        protected override void LateUpdateInternal() {
            OnLateUpdate?.Invoke();
        }
    }
}