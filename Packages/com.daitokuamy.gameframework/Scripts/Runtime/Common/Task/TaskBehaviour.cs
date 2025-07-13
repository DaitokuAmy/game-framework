using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// Updateに対応したMonoBehaviour
    /// </summary>
    public abstract class TaskBehaviour : MonoBehaviour, ITask, ITaskEventHandler {
        private TaskRunner _taskRunner;

        // Taskが有効状態か
        public virtual bool IsActive => isActiveAndEnabled;

        /// <summary>
        /// 生成時処理(override用)
        /// </summary>
        protected virtual void AwakeInternal() {
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void OnDestroyInternal() {
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

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            AwakeInternal();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            OnDestroyInternal();

            if (_taskRunner != null) {
                _taskRunner.Unregister(this);
                _taskRunner = null;
            }
        }
    }
}