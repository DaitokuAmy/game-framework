using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// Updateに対応したMonoBehaviour
    /// </summary>
    public abstract class UpdatableMonoBehaviour : MonoBehaviour, IUpdatable, IUpdatableEventHandler {
        private UpdateScheduler _updateScheduler;

        /// <summary>有効状態</summary>
        public virtual bool IsActive => isActiveAndEnabled;

        /// <inheritdoc/>
        void IUpdatable.Update() {
            UpdateInternal();
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IUpdatable>.OnRegistered(UpdateScheduler updateScheduler) {
            _updateScheduler = updateScheduler;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IUpdatable>.OnUnregistered(UpdateScheduler updateScheduler) {
            if (updateScheduler == _updateScheduler) {
                _updateScheduler = null;
            }
        }

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
        /// 更新処理(override用)
        /// </summary>
        protected virtual void UpdateInternal() {
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

            if (_updateScheduler != null) {
                _updateScheduler.UnregisterUpdatable(this);
                _updateScheduler = null;
            }
        }
    }
}
