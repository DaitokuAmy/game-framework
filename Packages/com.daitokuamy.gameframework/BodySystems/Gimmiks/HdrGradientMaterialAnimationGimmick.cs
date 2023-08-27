using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// HdrGradient型のMaterialアニメーションギミック
    /// </summary>
    public class HdrGradientMaterialAnimationGimmick : MaterialAnimationGimmick {
        [SerializeField, Tooltip("グラデーション"), ColorUsage(true, true)]
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