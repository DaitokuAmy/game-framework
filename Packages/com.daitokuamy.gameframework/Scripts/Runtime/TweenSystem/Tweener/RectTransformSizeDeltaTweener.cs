using UnityEngine;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// RectTransformのsizeDeltaをtoへ補間するTweener
    /// </summary>
    public sealed class RectTransformSizeDeltaTweener : Tweener {
        private RectTransform _target = null!;
        private Vector2 _from;
        private Vector2 _to;

        /// <summary>
        /// 初期化（fromはBegin時にキャプチャ）
        /// </summary>
        public RectTransformSizeDeltaTweener Setup(RectTransform target, Vector2 to, float duration) {
            _target = target;
            _to = to;
            SetupDuration(duration);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            _from = _target != null ? _target.sizeDelta : default;
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            if (_target == null) {
                return;
            }

            _target.sizeDelta = Vector2.LerpUnclamped(_from, _to, eased);
        }

        /// <inheritdoc/>
        protected override void OnResetTweener() {
            _target = null!;
            _from = default;
            _to = default;
        }
    }
}
