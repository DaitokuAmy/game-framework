using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// Materialの値を設定できるギミック基底
    /// </summary>
    public abstract class MaterialStateGimmick<T> : StateGimmickBase<MaterialStateGimmick<T>.StateInfo> {
        // 更新モード
        private enum UpdateMode {
            Update,
            LateUpdate,
        }

        /// <summary>
        /// ステート情報基底
        /// </summary>
        [Serializable]
        public class StateInfo : StateInfoBase {
            [Tooltip("対象の値")]
            public T materialValue;
        }
        
        [SerializeField, Tooltip("制御プロパティ名")]
        private string _propertyName = "";
        [SerializeField, Tooltip("対象のMaterial")]
        private RendererMaterial[] _targets = Array.Empty<RendererMaterial>();
        [SerializeField, Tooltip("Materialの制御タイプ")]
        private MaterialInstance.ControlType _controlType = MaterialInstance.ControlType.Auto;
        [SerializeField, Tooltip("ブレンド時間")]
        private float _blendDuration = 0.2f;
        [SerializeField, Tooltip("更新モード")]
        private UpdateMode _updateMode = UpdateMode.LateUpdate;
        
        // マテリアルインスタンスリスト
        private List<MaterialInstance> _materialInstances = new();
        // マテリアル制御ハンドル
        private MaterialHandle _materialHandle;
        // プロパティのID
        private int _propertyId;
        // 残り時間
        private float _timer;
        // ターゲット値
        private T _targetValue;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            base.InitializeInternal();
            Refresh();
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            DestroyMaterialInstances();
            base.DisposeInternal();
        }

        /// <inheritdoc/>
        protected override void OnValidateInternal() {
            base.OnValidateInternal();
            Refresh();
        }

        /// <inheritdoc/>
        protected sealed override void ChangeState(StateInfo prev, StateInfo current, bool immediate) {
            if (current == null) {
                _timer = -1.0f;
                return;
            }

            _targetValue = current.materialValue;

            if (immediate || prev == null) {
                _timer = -1.0f;
                SetValue(_targetValue, 1.0f, _materialHandle, _propertyId);
                return;
            }

            _timer = _blendDuration;
        }

        /// <inheritdoc/>
        protected sealed override void UpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.Update) {
                UpdateValue(deltaTime);
            }
        }

        /// <inheritdoc/>
        protected sealed override void LateUpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.LateUpdate) {
                UpdateValue(deltaTime);
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
            DestroyMaterialInstances();
            _materialInstances = _targets
                .Where(x => x.IsValid)
                .Select(x =>
                    new MaterialInstance(x.renderer, x.materialIndex, _controlType))
                .ToList();
            _materialHandle = new MaterialHandle(_materialInstances);
            _propertyId = Shader.PropertyToID(_propertyName);
        }

        /// <summary>
        /// Materialインスタンスの削除
        /// </summary>
        private void DestroyMaterialInstances() {
            foreach (var instance in _materialInstances) {
                instance.Dispose();
            }

            _materialInstances.Clear();
        }
    }
}
