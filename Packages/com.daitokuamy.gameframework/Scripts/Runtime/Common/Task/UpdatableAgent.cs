using System;

namespace GameFramework {
    /// <summary>
    /// Updatable代替処理用Agent
    /// </summary>
    public sealed class UpdatableAgent : DisposableUpdatable {
        private bool _active = true;
        
        /// <summary>更新通知</summary>
        public event Action UpdateEvent;

        /// <inheritdoc/>
        public override bool IsActive => _active;
        
        /// <inheritdoc/>
        protected override void UpdateInternal() {
            UpdateEvent?.Invoke();
        }
        
        /// <summary>
        /// アクティブ状態の切り替え
        /// </summary>
        public void SetActive(bool active) {
            _active = active;
        }
    }
}