using UnityEngine;

namespace GameFramework.UISystem {
    /// <summary>
    /// AnchoredPositionコントロールするTouchAnimation
    /// </summary>
    public class AnchoredPositionTouchAnimation : TouchAnimation {
        [Space]
        [SerializeField, Tooltip("TouchDown時のAnchoredPosition値")]
        private Vector2 _downAnchoredPosition = new(0, -10);
        [SerializeField, Tooltip("TouchUp時のAnchoredPosition値")]
        private Vector2 _upAnchoredPosition = new(0, 0);

        /// <inheritdoc/>
        protected override void ApplyAnimation(bool isDown, float ratio) {
            var targetPos = isDown ? _downAnchoredPosition : _upAnchoredPosition;
            var currentPos = Target.anchoredPosition;
            var nextPos = Vector2.Lerp(currentPos, targetPos, ratio);
            Target.anchoredPosition = nextPos;
        }
    }
}
