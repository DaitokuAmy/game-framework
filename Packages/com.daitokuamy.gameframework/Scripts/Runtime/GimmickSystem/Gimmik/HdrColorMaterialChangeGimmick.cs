using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// MaterialのColor値を設定できるギミック基底
    /// </summary>
    public class HdrColorMaterialChangeGimmick : MaterialChangeGimmick<HdrColor> {
        /// <inheritdoc/>
        protected override void SetValue(MaterialHandle handle, int propertyId, HdrColor val, float rate) {
            var current = handle.GetColor(propertyId);
            current = Color.Lerp(current, val, rate);
            handle.SetColor(propertyId, current);
        }
    }
}
