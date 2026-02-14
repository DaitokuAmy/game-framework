using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// Graphicのalphaをtoへ補間するTweener
    /// </summary>
    public sealed class GraphicAlphaTweener : Tweener {
        private Graphic _target = null!;
        private float _from;
        private float _to;

        /// <summary>
        /// 初期化（fromはBegin時にキャプチャ）
        /// </summary>
        public GraphicAlphaTweener Setup(Graphic target, float to, float duration) {
            _target = target;
            _to = to;
            SetupDuration(duration);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            _from = _target != null ? _target.color.a : 0.0f;
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            if (_target == null) {
                return;
            }

            var color = _target.color;
            color.a = Mathf.LerpUnclamped(_from, _to, eased);
            _target.color = color;
        }

        /// <inheritdoc/>
        protected override void OnResetTweener() {
            _target = null!;
            _from = 0.0f;
            _to = 0.0f;
        }
    }
}

