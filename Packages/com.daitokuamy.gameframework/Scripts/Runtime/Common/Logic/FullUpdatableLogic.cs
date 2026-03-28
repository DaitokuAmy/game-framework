namespace GameFramework {
    /// <summary>
    /// Update/LateUpdate/FixedUpdatable可能なロジック
    /// </summary>
    public abstract class FullUpdatableLogic : Logic, IUpdatable, IUpdatableEventHandler, ILateUpdatable, ILateUpdatableEventHandler, IFixedUpdatable, IFixedUpdatableEventHandler {
        private UpdateScheduler _updateScheduler;
        private UpdateScheduler _lateUpdateScheduler;
        private UpdateScheduler _fixedUpdateScheduler;

        /// <inheritdoc/>
        bool IUpdatableBase.IsActive => IsActive;

        /// <inheritdoc/>
        private protected override void SystemDisposeInternal() {
            // 登録を除外
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
        void IUpdatableEventHandlerBase<IUpdatable>.OnRegistered(UpdateScheduler runner) {
            _updateScheduler = runner;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IUpdatable>.OnUnregistered(UpdateScheduler runner) {
            if (runner == _updateScheduler) {
                _updateScheduler = null;
            }
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnRegistered(UpdateScheduler runner) {
            _lateUpdateScheduler = runner;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnUnregistered(UpdateScheduler runner) {
            if (runner == _lateUpdateScheduler) {
                _lateUpdateScheduler = null;
            }
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IFixedUpdatable>.OnRegistered(UpdateScheduler runner) {
            _fixedUpdateScheduler = runner;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IFixedUpdatable>.OnUnregistered(UpdateScheduler runner) {
            if (runner == _fixedUpdateScheduler) {
                _fixedUpdateScheduler = null;
            }
        }

        /// <summary>
        /// 更新処理(Override用)
        /// </summary>
        protected virtual void UpdateInternal() {
        }

        /// <summary>
        /// 後更新処理(Override用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }

        /// <summary>
        /// 固定更新処理(Override用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
        }
    }
}
