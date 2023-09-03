using System;
using System.Linq;
using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Materialの値をアニメーションさせるギミック基底
    /// </summary>
    public abstract class MaterialAnimationGimmick : AnimationGimmick {
        [SerializeField, Tooltip("制御プロパティ名")]
        private string _propertyName = "";
        [SerializeField, Tooltip("対象のMaterial")]
        private RendererMaterial[] _targets = Array.Empty<RendererMaterial>();
        [SerializeField, Tooltip("Materialの制御タイプ")]
        private MaterialInstance.ControlType _controlType = MaterialInstance.ControlType.Auto;
        [SerializeField, Tooltip("再生時間")]
        private float _duration = 1.0f;
        [SerializeField, Tooltip("ループ再生するか")]
        private bool _looping;

        // マテリアル制御ハンドル
        private MaterialHandle _materialHandle;
        // プロパティのID
        private int _propertyId;

        // トータル時間
        public override float Duration => _duration;
        // ループ再生するか
        public override bool IsLooping => _looping;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();
            Refresh();
        }

        /// <summary>
        /// 再生状態の反映
        /// </summary>
        protected override void Evaluate(float time) {
            var ratio = Duration > float.Epsilon ? Mathf.Clamp01(time / Duration) : 1.0f;
            SetValue(_materialHandle, Shader.PropertyToID(_propertyName), ratio);
        }

        /// <summary>
        /// Validate処理
        /// </summary>
        protected override void OnValidateInternal() {
            base.OnValidateInternal();
            Refresh();
        }

        /// <summary>
        /// 値の更新
        /// </summary>
        protected abstract void SetValue(MaterialHandle handle, int propertyId, float ratio);

        /// <summary>
        /// Material情報のリフレッシュ
        /// </summary>
        private void Refresh() {
            var instances = _targets
                .Where(x => x.IsValid)
                .Select(x =>
                    new MaterialInstance(x.renderer, x.materialIndex, _controlType));
            _materialHandle = new MaterialHandle(instances);
            _propertyId = Shader.PropertyToID(_propertyName);
        }
    }
}