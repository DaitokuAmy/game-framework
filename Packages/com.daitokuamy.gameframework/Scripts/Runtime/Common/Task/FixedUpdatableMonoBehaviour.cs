using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// FixedUpdateに対応したMonoBehaviour
    /// </summary>
    public abstract class FixedUpdatableMonoBehaviour : MonoBehaviour, IFixedUpdatable, IFixedUpdatableEventHandler {
        private UpdateScheduler _updateScheduler;

        /// <summary>有効状態</summary>
        public virtual bool IsActive => isActiveAndEnabled;

        /// <inheritdoc/>
        void IFixedUpdatable.Update() {
            FixedUpdateInternal();
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IFixedUpdatable>.OnRegistered(UpdateScheduler updateScheduler) {
            _updateScheduler = updateScheduler;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IFixedUpdatable>.OnUnregistered(UpdateScheduler updateScheduler) {
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
        /// 固定更新処理(override用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
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
                _updateScheduler.UnregisterFixedUpdatable(this);
                _updateScheduler = null;
            }
        }
    }
}
