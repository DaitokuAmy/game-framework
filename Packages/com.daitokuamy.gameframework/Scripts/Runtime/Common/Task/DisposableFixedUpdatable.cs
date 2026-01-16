using System;

namespace GameFramework {
    /// <summary>
    /// FixedUpdate実装基底クラス
    /// </summary>
    public abstract class DisposableFixedUpdatable : IFixedUpdatable, IFixedUpdatableEventHandler, IDisposable {
        private UpdateScheduler _updateScheduler;

        /// <inheritdoc/>
        public virtual bool IsActive => true;

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            DisposeInternal();

            if (_updateScheduler != null) {
                _updateScheduler.UnregisterFixedUpdatable(this);
                _updateScheduler = null;
            }
        }

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
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 後更新処理(override用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
        }
    }
}