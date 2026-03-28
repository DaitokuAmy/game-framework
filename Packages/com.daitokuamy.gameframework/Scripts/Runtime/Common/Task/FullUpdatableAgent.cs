using System;

namespace GameFramework {
    /// <summary>
    /// FullUpdatable代替処理用Agent
    /// </summary>
    public sealed class FullUpdatableAgent : DisposableFullUpdatable {
        private bool _active = true;
        
        /// <summary>更新通知</summary>
        public event Action UpdateEvent;
        /// <summary>後更新通知</summary>
        public event Action LateUpdateEvent;
        /// <summary>固定更新通知</summary>
        public event Action FixedUpdateEvent;

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
        
        /// <inheritdoc/>
        protected override void FixedUpdateInternal() {
            FixedUpdateEvent?.Invoke();
        }
        
        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        public void SetActive(bool active) {
            _active = active;
        }
    }
}
