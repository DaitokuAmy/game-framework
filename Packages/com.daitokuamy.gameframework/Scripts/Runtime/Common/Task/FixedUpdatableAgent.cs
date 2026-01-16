using System;

namespace GameFramework {
    /// <summary>
    /// FixedUpdatable代替処理用Agent
    /// </summary>
    public sealed class FixedUpdatableAgent : DisposableFixedUpdatable {
        private bool _active = true;
        
        /// <summary>固定更新通知</summary>
        public event Action FixedUpdateEvent;

        /// <inheritdoc/>
        public override bool IsActive => _active;
        
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