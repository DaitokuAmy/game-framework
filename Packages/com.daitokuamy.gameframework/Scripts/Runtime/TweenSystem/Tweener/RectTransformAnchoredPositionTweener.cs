using UnityEngine;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// RectTransformのanchoredPositionをtoへ補間するTweener
    /// </summary>
    public sealed class RectTransformAnchoredPositionTweener : Tweener {
        private RectTransform _target = null!;
        private Vector2 _from;
        private Vector2 _to;

        /// <summary>
        /// 初期化（fromはBegin時にキャプチャ）
        /// </summary>
        public RectTransformAnchoredPositionTweener Setup(RectTransform target, Vector2 to, float duration) {
            _target = target;
            _to = to;
            SetupDuration(duration);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            _from = _target != null ? _target.anchoredPosition : default;
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            if (_target == null) {
                return;
            }

            _target.anchoredPosition = Vector2.LerpUnclamped(_from, _to, eased);
        }

        /// <inheritdoc/>
        protected override void OnResetTweener() {
            _target = null!;
            _from = default;
            _to = default;
        }
    }
}
