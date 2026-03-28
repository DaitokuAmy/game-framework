using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// LateUpdateに対応したMonoBehaviour
    /// </summary>
    public abstract class LateUpdatableMonoBehaviour : MonoBehaviour, ILateUpdatable, ILateUpdatableEventHandler {
        private UpdateScheduler _updateScheduler;

        /// <summary>有効状態</summary>
        public virtual bool IsActive => isActiveAndEnabled;

        /// <inheritdoc/>
        void ILateUpdatable.Update() {
            LateUpdateInternal();
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnRegistered(UpdateScheduler updateScheduler) {
            _updateScheduler = updateScheduler;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnUnregistered(UpdateScheduler updateScheduler) {
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
        /// 後更新処理(override用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
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
                _updateScheduler.UnregisterLateUpdatable(this);
                _updateScheduler = null;
            }
        }
    }
}
