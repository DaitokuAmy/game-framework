using System;

namespace GameFramework.TaskSystems {
    /// <summary>
    /// タスク用インターフェース
    /// </summary>
    public abstract class DisposableTask : ITask, ITaskEventHandler, IDisposable {
        private TaskRunner _taskRunner;

        // Taskが有効状態か
        public virtual bool IsActive => true;

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