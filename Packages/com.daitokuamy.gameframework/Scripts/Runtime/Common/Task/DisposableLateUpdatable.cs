using System;

namespace GameFramework {
    /// <summary>
    /// LateUpdate実装基底クラス
    /// </summary>
    public abstract class DisposableLateUpdatable : ILateUpdatable, ILateUpdatableEventHandler, IDisposable {
        private UpdateScheduler _updateScheduler;

        /// <inheritdoc/>
        public virtual bool IsActive => true;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            DisposeInternal();

            if (_updateScheduler != null) {
                _updateScheduler.UnregisterLateUpdatable(this);
                _updateScheduler = null;
            }
        }

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
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 後更新処理(override用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }
    }
}