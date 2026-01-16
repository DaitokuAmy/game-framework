using System;

namespace GameFramework {
    /// <summary>
    /// Update/LateUpdate実装基底クラス
    /// </summary>
    public abstract class DisposableUpdateAndLateUpdatable : IUpdatable, IUpdatableEventHandler, ILateUpdatable, ILateUpdatableEventHandler, IDisposable {
        private UpdateScheduler _updateScheduler;
        private UpdateScheduler _lateUpdateScheduler;

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
    }
}