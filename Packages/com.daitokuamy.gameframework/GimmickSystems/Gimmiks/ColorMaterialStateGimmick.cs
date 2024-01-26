using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// MaterialのColor値を設定できるギミック基底
    /// </summary>
    public class ColorMaterialStateGimmick : MaterialStateGimmick<Color> {
        /// <summary>
        /// マテリアルの値変更
        /// </summary>
        protected override void SetValue(Color targetValue, float ratio, MaterialHandle materialHandle, int propertyId) {
            var current = materialHandle.GetColor(propertyId);
            current = Color.Lerp(current, targetValue, ratio);
            materialHandle.SetColor(propertyId, current);
        }
    }
}