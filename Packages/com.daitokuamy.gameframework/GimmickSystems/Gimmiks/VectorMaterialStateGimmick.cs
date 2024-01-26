using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// MaterialのVector値を設定できるギミック基底
    /// </summary>
    public class VectorMaterialStateGimmick : MaterialStateGimmick<Vector4> {
        /// <summary>
        /// マテリアルの値変更
        /// </summary>
        protected override void SetValue(Vector4 targetValue, float ratio, MaterialHandle materialHandle, int propertyId) {
            var current = materialHandle.GetVector(propertyId);
            current = Vector4.Lerp(current, targetValue, ratio);
            materialHandle.SetVector(propertyId, current);
        }
    }
}