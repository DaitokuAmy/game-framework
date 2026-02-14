using UnityEngine;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// CanvasGroupのalphaをtoへ補間するTweener
    /// </summary>
    public sealed class CanvasGroupAlphaTweener : Tweener {
        private CanvasGroup _target = null!;
        private float _from;
        private float _to;

        /// <summary>
        /// 初期化（fromはBegin時にキャプチャ）
        /// </summary>
        public CanvasGroupAlphaTweener Setup(CanvasGroup target, float to, float duration) {
            _target = target;
            _to = to;
            SetupDuration(duration);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            _from = _target != null ? _target.alpha : 0f;
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            if (_target == null) {
                return;
            }

            _target.alpha = Mathf.LerpUnclamped(_from, _to, eased);
        }

        /// <inheritdoc/>
        protected override void OnResetTweener() {
            _target = null!;
            _from = 0f;
            _to = 0f;
        }
    }
}
