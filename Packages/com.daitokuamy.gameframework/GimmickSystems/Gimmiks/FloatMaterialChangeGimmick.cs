using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// MaterialのFloat値を設定できるギミック基底
    /// </summary>
    public class FloatMaterialChangeGimmick : MaterialChangeGimmick<float> {
        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, float val, float rate) {
            var current = handle.GetFloat(propertyId);
            current = Mathf.Lerp(current, val, rate);
            handle.SetFloat(propertyId, current);
        }
    }
}