namespace GameFramework {
    /// <summary>
    /// FixedUpdate可能なロジック
    /// </summary>
    public abstract class FixedUpdatableLogic : Logic, IFixedUpdatable, IFixedUpdatableEventHandler {
        private UpdateScheduler _updateScheduler;

        /// <inheritdoc/>
        private protected override void SystemDisposeInternal() {
            // 登録を除外
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
        void IUpdatableEventHandlerBase<IFixedUpdatable>.OnRegistered(UpdateScheduler runner) {
            _updateScheduler = runner;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IFixedUpdatable>.OnUnregistered(UpdateScheduler runner) {
            if (runner == _updateScheduler) {
                _updateScheduler = null;
            }
        }

        /// <summary>
        /// 固定更新処理(Override用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
        }
    }
}
