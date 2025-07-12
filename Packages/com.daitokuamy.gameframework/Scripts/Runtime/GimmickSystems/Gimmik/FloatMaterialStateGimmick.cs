using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// MaterialのFloat値を設定できるギミック基底
    /// </summary>
    public class FloatMaterialStateGimmick : MaterialStateGimmick<float> {
        /// <summary>
        /// マテリアルの値変更
        /// </summary>
        protected override void SetValue(float targetValue, float ratio, MaterialHandle materialHandle, int propertyId) {
            var current = materialHandle.GetFloat(propertyId);
            current = Mathf.Lerp(current, targetValue, ratio);
            materialHandle.SetFloat(propertyId, current);
        }
    }
}