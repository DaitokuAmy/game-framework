using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// ImageのfillAmountをtoへ補間するTweener
    /// </summary>
    public sealed class ImageFillAmountTweener : Tweener {
        private Image _target = null!;
        private float _from;
        private float _to;

        /// <summary>
        /// 初期化（fromはBegin時にキャプチャ）
        /// </summary>
        public ImageFillAmountTweener Setup(Image target, float to, float duration) {
            _target = target;
            _to = to;
            SetupDuration(duration);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            _from = _target != null ? _target.fillAmount : 0.0f;
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            if (_target == null) {
                return;
            }

            _target.fillAmount = Mathf.LerpUnclamped(_from, _to, eased);
        }

        /// <inheritdoc/>
        protected override void OnResetTweener() {
            _target = null!;
            _from = 0.0f;
            _to = 0.0f;
        }
    }
}

