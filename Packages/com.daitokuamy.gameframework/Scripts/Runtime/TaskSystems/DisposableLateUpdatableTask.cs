using System;

namespace GameFramework.TaskSystems {
    /// <summary>
    /// タスク用インターフェース
    /// </summary>
    public abstract class DisposableLateUpdatableTask : ILateUpdatableTask, ITaskEventHandler, IDisposable {
        private TaskRunner _taskRunner;

        /// <summary>Taskが有効状態か</summary>
        bool ITask.IsActive => IsTaskActive;

        /// <summary>Taskが有効状態か</summary>
        protected virtual bool IsTaskActive => true;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            DisposeInternal();

            if (_taskRunner != null) {
                _taskRunner.Unregister(this);
                _taskRunner = null;
            }
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ITask.Update() {
            UpdateInternal();
        }

        /// <summary>
        /// 更新処理(override用)
        /// </summary>
        protected virtual void UpdateInternal() {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            LateUpdateInternal();
        }

        /// <summary>
        /// 後更新処理(override用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }

        /// <summary>
        /// Taskの登録通知
        /// </summary>
        void ITaskEventHandler.OnRegistered(TaskRunner taskRunner) {
            _taskRunner = taskRunner;
        }

        /// <summary>
        /// Taskの登録解除通知
        /// </summary>
        void ITaskEventHandler.OnUnregistered(TaskRunner taskRunner) {
            if (taskRunner == _taskRunner) {
                _taskRunner = null;
            }
        }
    }
}