using GameFramework.Core;
using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Vector型のMaterialアニメーションギミック
    /// </summary>
    public class VectorColorMaterialAnimationGimmick : MaterialAnimationGimmick {
        [SerializeField, Tooltip("開始値"), CopyableProperty]
        private Vector4 _start;
        [SerializeField, Tooltip("終了値"), CopyableProperty]
        private Vector4 _end;
        [SerializeField, Tooltip("カーブ"), CopyableProperty]
        private AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, float ratio) {
            ratio = Mathf.Clamp01(_curve.Evaluate(ratio));
            var val = Vector4.Lerp(_start, _end, ratio);
            handle.SetVector(propertyId, val);
        }
    }
}