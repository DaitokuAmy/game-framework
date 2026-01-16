namespace GameFramework {
    /// <summary>
    /// LateUpdate可能なロジック
    /// </summary>
    public abstract class LateUpdatableLogic : Logic, ILateUpdatable, ILateUpdatableEventHandler {
        private UpdateScheduler _updateScheduler;

        /// <inheritdoc/>
        private protected override void SystemDisposeInternal() {
            // 登録を除外
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
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnRegistered(UpdateScheduler runner) {
            _updateScheduler = runner;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnUnregistered(UpdateScheduler runner) {
            if (runner == _updateScheduler) {
                _updateScheduler = null;
            }
        }

        /// <summary>
        /// 後更新処理(Override用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }
    }
}