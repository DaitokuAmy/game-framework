using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Color型のMaterialアニメーションギミック
    /// </summary>
    public class ColorMaterialAnimationGimmick : MaterialAnimationGimmick {
        [SerializeField, Tooltip("開始値")]
        private Color _start;
        [SerializeField, Tooltip("終了値")]
        private Color _end;
        [SerializeField, Tooltip("カーブ")]
        private AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, float ratio) {
            ratio = Mathf.Clamp01(_curve.Evaluate(ratio));
            var val = Color.Lerp(_start, _end, ratio);
            handle.SetColor(propertyId, val);
        }
    }
}