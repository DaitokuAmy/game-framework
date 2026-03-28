using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// MaterialのColor値を設定できるギミック基底
    /// </summary>
    public class ColorMaterialStateGimmick : MaterialStateGimmick<Color> {
        /// <inheritdoc/>
        protected override void SetValue(Color targetValue, float ratio, MaterialHandle materialHandle, int propertyId) {
            var current = materialHandle.GetColor(propertyId);
            current = Color.Lerp(current, targetValue, ratio);
            materialHandle.SetColor(propertyId, current);
        }
    }
}
