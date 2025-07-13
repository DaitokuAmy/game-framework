using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// Float型のMaterialアニメーションギミック
    /// </summary>
    public class FloatMaterialAnimationGimmick : MaterialAnimationGimmick {
        [SerializeField, Tooltip("開始値")]
        private float _start;
        [SerializeField, Tooltip("終了値")]
        private float _end;
        [SerializeField, Tooltip("カーブ")]
        private AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, float ratio) {
            ratio = Mathf.Clamp01(_curve.Evaluate(ratio));
            var val = Mathf.Lerp(_start, _end, ratio);
            handle.SetFloat(propertyId, val);
        }
    }
}