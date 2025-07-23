using System;

namespace GameFramework {
    /// <summary>
    /// タスク用インターフェース
    /// </summary>
    public class TaskAgent : DisposableLateUpdatableTask {
        /// <summary>更新通知</summary>
        public event Action UpdateEvent;
        /// <summary>後更新通知</summary>
        public event Action LateUpdateEvent;

        /// <summary>Taskが有効か</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// タスク更新
        /// </summary>
        protected override void UpdateInternal() {
            UpdateEvent?.Invoke();
        }

        /// <summary>
        /// タスク後更新
        /// </summary>
        protected override void LateUpdateInternal() {
            LateUpdateEvent?.Invoke();
        }
    }
}