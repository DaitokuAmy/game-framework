using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// MaterialのVector値を設定できるギミック基底
    /// </summary>
    public class VectorMaterialChangeGimmick : MaterialChangeGimmick<Vector4> {
        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, Vector4 val, float rate) {
            var current = handle.GetVector(propertyId);
            current = Vector4.Lerp(current, val, rate);
            handle.SetVector(propertyId, current);
        }
    }
}