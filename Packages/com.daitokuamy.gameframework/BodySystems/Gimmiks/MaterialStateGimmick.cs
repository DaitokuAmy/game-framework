using System;
using System.Linq;
using GameFramework.Core;
using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Materialの値を設定できるギミック基底
    /// </summary>
    public abstract class MaterialStateGimmick<T> : StateGimmickBase<MaterialStateGimmick<T>.StateInfo> {
        /// <summary>
        /// ステート情報基底
        /// </summary>
        [Serializable]
        public class StateInfo : StateInfoBase {
            [Tooltip("対象の値"), CopyableProperty]
            public T materialValue;
        }
        
        [SerializeField, Tooltip("制御プロパティ名")]
        private string _propertyName = "";
        [SerializeField, Tooltip("対象のMaterial")]
        private RendererMaterial[] _targets;
        [SerializeField, Tooltip("Materialの制御タイプ")]
        private MaterialInstance.ControlType _controlType = MaterialInstance.ControlType.Auto;
        [SerializeField, Tooltip("ブレンド時間"), CopyableProperty]
        private float _blendDuration = 0.2f;

        // マテリアル制御ハンドル
        private MaterialHandle _materialHandle;
        // プロパティのID
        private int _propertyId;
        // 残り時間
        private float _timer;
        // ターゲット値
        private T _targetValue;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();
            Refresh();
        }

        /// <summary>
        /// Validate処理
        /// </summary>
        protected override void OnValidateInternal() {
            base.OnValidateInternal();
            Refresh();
        }

        /// <summary>
        /// ステートの変更処理
        /// </summary>
        /// <param name="prev">変更前のステート</param>
        /// <param name="current">変更後のステート</param>
        /// <param name="immediate">即時遷移するか</param>
        protected sealed override void ChangeState(StateInfo prev, StateInfo current, bool immediate) {
            if (current != null) {
                _targetValue = current.materialValue;
                _timer = !immediate && prev != null ? _blendDuration : 0.0f;
            }
        }

        /// <summary>
        /// マテリアルの値変更
        /// </summary>
        protected abstract void SetValue(T targetValue, float ratio, MaterialHandle materialHandle, int propertyId);

        /// <summary>
        /// 値の更新
        /// </summary>
        private void UpdateValue(float deltaTime) {
            if (_timer < 0.0f) {
                return;
            }

            // 現在の値に対しての反映率
            var ratio = _timer > 0.0001f ? Mathf.Clamp01(deltaTime / _timer) : 1.0f;
            SetValue(_targetValue, ratio, _materialHandle, _propertyId);

            // 時間更新
            _timer -= deltaTime;
        }

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