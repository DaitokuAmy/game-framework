using System;

namespace GameFramework {
    /// <summary>
    /// 廃棄可能Updatableの汎用基底
    /// </summary>
    public abstract class DisposableUpdatable : IUpdatable, IUpdatableEventHandler, IDisposable {
        private UpdateScheduler _updateScheduler;

        /// <inheritdoc/>
        public virtual bool IsActive => true;

        /// <inheritdoc/>
        public void Dispose() {
            DisposeInternal();

            if (_updateScheduler != null) {
                _updateScheduler.UnregisterUpdatable(this);
                _updateScheduler = null;
            }
        }

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
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 更新処理(override用)
        /// </summary>
        protected virtual void UpdateInternal() {
        }
    }
}
