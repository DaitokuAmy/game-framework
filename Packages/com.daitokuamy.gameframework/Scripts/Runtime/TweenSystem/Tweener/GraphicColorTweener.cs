using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// Graphicのcolorをtoへ補間するTweener
    /// </summary>
    public sealed class GraphicColorTweener : Tweener {
        private Graphic _target = null!;
        private Color _from;
        private Color _to;

        /// <summary>
        /// 初期化（fromはBegin時にキャプチャ）
        /// </summary>
        public GraphicColorTweener Setup(Graphic target, Color to, float duration) {
            _target = target;
            _to = to;
            SetupDuration(duration);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            _from = _target != null ? _target.color : default;
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            if (_target == null) {
                return;
            }

            _target.color = Color.LerpUnclamped(_from, _to, eased);
        }

        /// <inheritdoc/>
        protected override void OnResetTweener() {
            _target = null!;
            _from = default;
            _to = default;
        }
    }
}
