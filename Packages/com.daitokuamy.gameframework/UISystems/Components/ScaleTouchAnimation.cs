using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// ScaleコントロールするTouchAnimation
    /// </summary>
    public class ScaleTouchAnimation : TouchAnimation {
        [Space]
        [SerializeField, Tooltip("TouchDown時のスケール値")]
        private float _downScale = 0.9f;
        [SerializeField, Tooltip("TouchUp時のスケール値")]
        private float _upScale = 1.0f;

        /// <summary>TouchDown時のScale</summary>
        public float DownScale {
            get => _downScale;
            set => _downScale = value;
        }
        /// <summary>TouchUp時のScale</summary>
        public float UpScale {
            get => _upScale;
            set => _upScale = value;
        }

        /// <summary>
        /// アニメーションの適用
        /// </summary>
        /// <param name="isDown">TouchDown中か</param>
        /// <param name="ratio">補間割合</param>
        protected override void ApplyAnimation(bool isDown, float ratio) {
            var targetScale = isDown ? _downScale : _upScale;
            var currentScale = Target.localScale.x;
            var nextScale = Mathf.Lerp(currentScale, targetScale, ratio);
            Target.localScale = new Vector3(nextScale, nextScale, 1.0f);
        }
    }
}