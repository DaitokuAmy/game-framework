using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// MaterialのColor値を設定できるギミック基底
    /// </summary>
    public class ColorMaterialChangeGimmick : MaterialChangeGimmick<Color> {
        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, Color val, float rate) {
            var current = handle.GetColor(propertyId);
            current = Color.Lerp(current, val, rate);
            handle.SetColor(propertyId, current);
        }
    }
}