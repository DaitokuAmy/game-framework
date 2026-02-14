using UnityEngine;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// TransformのlocalScaleをtoへ補間するTweener
    /// </summary>
    public sealed class ScaleToTweener : Tweener {
        private Transform _target = null!;
        private Vector3 _from;
        private Vector3 _to;

        /// <summary>
        /// 初期化（fromはBegin時にキャプチャ）
        /// </summary>
        public ScaleToTweener Setup(Transform target, Vector3 to, float duration) {
            _target = target;
            _to = to;
            SetupDuration(duration);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            _from = _target != null ? _target.localScale : default;
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            if (_target == null) {
                return;
            }

            _target.localScale = Vector3.LerpUnclamped(_from, _to, eased);
        }

        /// <inheritdoc/>
        protected override void OnResetTweener() {
            _target = null!;
            _from = default;
            _to = default;
        }
    }
}
