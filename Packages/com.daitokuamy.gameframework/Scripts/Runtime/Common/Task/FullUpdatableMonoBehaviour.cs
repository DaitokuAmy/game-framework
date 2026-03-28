using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// Update/LateUpdate/FixedUpdateに対応したMonoBehaviour
    /// </summary>
    public abstract class FullUpdatableMonoBehaviour : MonoBehaviour, IUpdatable, IUpdatableEventHandler, ILateUpdatable, ILateUpdatableEventHandler, IFixedUpdatable, IFixedUpdatableEventHandler {
        private UpdateScheduler _updateScheduler;
        private UpdateScheduler _lateUpdateScheduler;
        private UpdateScheduler _fixedUpdateScheduler;

        /// <summary>有効状態</summary>
        public virtual bool IsActive => isActiveAndEnabled;

        /// <inheritdoc/>
        // ReSharper disable once Unity.DuplicateEventFunction
        void IUpdatable.Update() {
            UpdateInternal();
        }

        /// <inheritdoc/>
        // ReSharper disable once Unity.DuplicateEventFunction
        void ILateUpdatable.Update() {
            LateUpdateInternal();
        }

        /// <inheritdoc/>
        // ReSharper disable once Unity.DuplicateEventFunction
        void IFixedUpdatable.Update() {
            FixedUpdateInternal();
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

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnRegistered(UpdateScheduler updateScheduler) {
            _lateUpdateScheduler = updateScheduler;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnUnregistered(UpdateScheduler updateScheduler) {
            if (updateScheduler == _lateUpdateScheduler) {
                _lateUpdateScheduler = null;
            }
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IFixedUpdatable>.OnRegistered(UpdateScheduler updateScheduler) {
            _fixedUpdateScheduler = updateScheduler;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IFixedUpdatable>.OnUnregistered(UpdateScheduler updateScheduler) {
            if (updateScheduler == _fixedUpdateScheduler) {
                _fixedUpdateScheduler = null;
            }
        }

        /// <summary>
        /// 更新処理(override用)
        /// </summary>
        protected virtual void UpdateInternal() {
        }

        /// <summary>
        /// 後更新処理(override用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }

        /// <summary>
        /// 固定更新処理(override用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
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

            if (_lateUpdateScheduler != null) {
                _lateUpdateScheduler.UnregisterLateUpdatable(this);
                _lateUpdateScheduler = null;
            }

            if (_fixedUpdateScheduler != null) {
                _fixedUpdateScheduler.UnregisterFixedUpdatable(this);
                _fixedUpdateScheduler = null;
            }
        }
    }
}
