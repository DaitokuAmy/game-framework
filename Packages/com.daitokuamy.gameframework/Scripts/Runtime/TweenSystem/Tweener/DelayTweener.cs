namespace GameFramework.TweenSystem {
    /// <summary>
    /// Delay用Tweener
    /// </summary>
    internal sealed class DelayTweener : Tweener {
        /// <summary>
        /// 初期化
        /// </summary>
        public DelayTweener Setup(float seconds) {
            SetupDuration(seconds);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            // 何もしない
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            // Delayは何もしない
        }
    }
}
