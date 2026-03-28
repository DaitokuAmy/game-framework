using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// MaterialのColor値を設定できるギミック基底
    /// </summary>
    public class HdrColorMaterialStateGimmick : MaterialStateGimmick<HdrColor> {
        /// <inheritdoc/>
        protected override void SetValue(HdrColor targetValue, float ratio, MaterialHandle materialHandle, int propertyId) {
            var current = materialHandle.GetColor(propertyId);
            current = Color.Lerp(current, targetValue, ratio);
            materialHandle.SetColor(propertyId, current);
        }
    }
}
