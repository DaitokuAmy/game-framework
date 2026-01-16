using System;

namespace GameFramework {
    /// <summary>
    /// Update/LateUpdate/FixedUpdate実装基底クラス
    /// </summary>
    public abstract class DisposableFullUpdatable : IUpdatable, IUpdatableEventHandler, ILateUpdatable, ILateUpdatableEventHandler, IFixedUpdatable, IFixedUpdatableEventHandler, IDisposable {
        private UpdateScheduler _updateScheduler;
        private UpdateScheduler _lateUpdateScheduler;
        private UpdateScheduler _fixedUpdateScheduler;

        /// <inheritdoc/>
        public virtual bool IsActive => true;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            DisposeInternal();

            if (_updateScheduler != null) {
                _updateScheduler.UnregisterUpdatable(this);
                _updateScheduler = null;
            }

            if (_lateUpdateScheduler != null) {
                _lateUpdateScheduler.UnregisterLateUpdatable(this);
                _lateUpdateScheduler = null;
            }

            if (_fixedUpdateScheduler != null) {
                _fixedUpdateScheduler.UnregisterLateUpdatable(this);
                _fixedUpdateScheduler = null;
            }
        }

        /// <inheritdoc/>
        void IUpdatable.Update() {
            UpdateInternal();
        }

        /// <inheritdoc/>
        void ILateUpdatable.Update() {
            LateUpdateInternal();
        }

        /// <inheritdoc/>
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
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void DisposeInternal() {
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
    }
}