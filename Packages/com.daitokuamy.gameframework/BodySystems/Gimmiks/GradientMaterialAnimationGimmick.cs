using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Gradient型のMaterialアニメーションギミック
    /// </summary>
    public class GradientMaterialAnimationGimmick : MaterialAnimationGimmick {
        [SerializeField, Tooltip("グラデーション")]
        private Gradient _gradient;

        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, float ratio) {
            var val = _gradient.Evaluate(ratio);
            handle.SetColor(propertyId, val);
        }
    }
}