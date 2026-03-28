using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// Gradient型のMaterialアニメーションギミック
    /// </summary>
    public class GradientMaterialAnimationGimmick : MaterialAnimationGimmick {
        [SerializeField, Tooltip("グラデーション")]
        private Gradient _gradient;

        /// <inheritdoc/>
        protected override void SetValue(MaterialHandle handle, int propertyId, float ratio) {
            var val = _gradient.Evaluate(ratio);
            handle.SetColor(propertyId, val);
        }
    }
}
