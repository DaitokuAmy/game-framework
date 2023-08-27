using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// AnchoredPositionコントロールするTouchAnimation
    /// </summary>
    public class AnchoredPositionTouchAnimation : TouchAnimation {
        [Space]
        [SerializeField, Tooltip("TouchDown時のAnchoredPosition値")]
        private Vector2 _downAnchoredPosition = new(0, -10);
        [SerializeField, Tooltip("TouchUp時のAnchoredPosition値")]
        private Vector2 _upAnchoredPosition = new(0, 0);

        /// <summary>
        /// アニメーションの適用
        /// </summary>
        /// <param name="isDown">TouchDown中か</param>
        /// <param name="ratio">補間割合</param>
        protected override void ApplyAnimation(bool isDown, float ratio) {
            var targetPos = isDown ? _downAnchoredPosition : _upAnchoredPosition;
            var currentPos = Target.anchoredPosition;
            var nextPos = Vector2.Lerp(currentPos, targetPos, ratio);
            Target.anchoredPosition = nextPos;
        }
    }
}