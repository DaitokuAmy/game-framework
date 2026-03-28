using System;

namespace GameFramework {
    /// <summary>
    /// Update/LateUpdate代替処理用Agent
    /// </summary>
    public sealed class UpdateAndLateUpdatableAgent : DisposableUpdateAndLateUpdatable {
        private bool _active = true;
        
        /// <summary>更新通知</summary>
        public event Action UpdateEvent;
        /// <summary>後更新通知</summary>
        public event Action LateUpdateEvent;

        /// <inheritdoc/>
        public override bool IsActive => _active;
        
        /// <inheritdoc/>
        protected override void UpdateInternal() {
            UpdateEvent?.Invoke();
        }
        
        /// <inheritdoc/>
        protected override void LateUpdateInternal() {
            LateUpdateEvent?.Invoke();
        }
        
        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        public void SetActive(bool active) {
            _active = active;
        }
    }
}
