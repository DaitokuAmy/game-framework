using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// MaterialのFloat値を設定できるギミック基底
    /// </summary>
    public class FloatMaterialStateGimmick : MaterialStateGimmick<float> {
        /// <inheritdoc/>
        protected override void SetValue(float targetValue, float ratio, MaterialHandle materialHandle, int propertyId) {
            var current = materialHandle.GetFloat(propertyId);
            current = Mathf.Lerp(current, targetValue, ratio);
            materialHandle.SetFloat(propertyId, current);
        }
    }
}
