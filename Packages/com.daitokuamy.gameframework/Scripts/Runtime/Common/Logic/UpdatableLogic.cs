namespace GameFramework {
    /// <summary>
    /// Update可能なロジック
    /// </summary>
    public abstract class UpdatableLogic : Logic, IUpdatable, IUpdatableEventHandler {
        private UpdateScheduler _updateScheduler;
        
        /// <inheritdoc/>
        bool IUpdatableBase.IsActive => IsActive;

        /// <inheritdoc/>
        private protected override void SystemDisposeInternal() {
            // 登録を除外
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
        void IUpdatableEventHandlerBase<IUpdatable>.OnRegistered(UpdateScheduler runner) {
            _updateScheduler = runner;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IUpdatable>.OnUnregistered(UpdateScheduler runner) {
            if (runner == _updateScheduler) {
                _updateScheduler = null;
            }
        }

        /// <summary>
        /// 更新処理(Override用)
        /// </summary>
        protected virtual void UpdateInternal() {
        }
    }
}
