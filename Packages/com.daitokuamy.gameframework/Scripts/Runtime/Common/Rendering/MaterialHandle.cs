using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// マテリアル制御ハンドル
    /// </summary>
    public struct MaterialHandle {
        private MaterialInstance[] _instances;

        // 有効なハンドルか
        public bool IsValid => _instances != null && _instances.Length > 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="instances">制御対象のMaterial情報リスト</param>
        public MaterialHandle(IEnumerable<MaterialInstance> instances) {
            _instances = instances.ToArray();
        }

        /// <summary>
        /// MaterialInstanceのリストを取得(Debug用)
        /// </summary>
        public MaterialInstance[] GetMaterialInstances() {
            return _instances;
        }

        /// <summary>
        /// 各種セッター
        /// </summary>
        public void SetFloat(int nameId, float val) {
            SetValue(nameId, val, (x, id, v) => x.SetFloat(id, v));
        }

        public void SetInt(int nameId, int val) {
            SetValue(nameId, val, (x, id, v) => x.SetInt(id, v));
        }

        public void SetVector(int nameId, Vector4 val) {
            SetValue(nameId, val, (x, id, v) => x.SetVector(id, v));
        }

        public void SetColor(int nameId, Color val) {
            SetValue(nameId, val, (x, id, v) => x.SetColor(id, v));
        }

        public void SetMatrix(int nameId, Matrix4x4 val) {
            SetValue(nameId, val, (x, id, v) => x.SetMatrix(id, v));
        }

        public void SetFloatArray(int nameId, float[] val) {
            SetValue(nameId, val, (x, id, v) => x.SetFloatArray(id, v));
        }

        public void SetVectorArray(int nameId, Vector4[] val) {
            SetValue(nameId, val, (x, id, v) => x.SetVectorArray(id, v));
        }

        public void SetMatrixArray(int nameId, Matrix4x4[] val) {
            SetValue(nameId, val, (x, id, v) => x.SetMatrixArray(id, v));
        }

        public void SetTexture(int nameId, Texture val) {
            SetValue(nameId, val, (x, id, v) => x.SetTexture(id, v));
        }

        /// <summary>
        /// 各種ゲッター
        /// </summary>
        public float GetFloat(int nameId) {
            return GetValue(nameId, (x, id) => x.GetFloat(id));
        }

        public int GetInt(int nameId) {
            return GetValue(nameId, (x, id) => x.GetInt(id));
        }

        public Vector4 GetVector(int nameId) {
            return GetValue(nameId, (x, id) => x.GetVector(id));
        }

        public Color GetColor(int nameId) {
            return GetValue(nameId, (x, id) => x.GetColor(id));
        }

        public Matrix4x4 GetMatrix(int nameId) {
            return GetValue(nameId, (x, id) => x.GetMatrix(id));
        }

        public float[] GetFloatArray(int nameId) {
            return GetValue(nameId, (x, id) => x.GetFloatArray(id));
        }

        public Vector4[] GetVectorArray(int nameId) {
            return GetValue(nameId, (x, id) => x.GetVectorArray(id));
        }

        public Matrix4x4[] GetMatrixArray(int nameId) {
            return GetValue(nameId, (x, id) => x.GetMatrixArray(id));
        }

        public Texture GetTexture(int nameId) {
            return GetValue(nameId, (x, id) => x.GetTexture(id));
        }

        /// <summary>
        /// Materialに適切な方法で値を設定する
        /// </summary>
        private void SetValue<T>(int nameId, T val, Action<MaterialInstance, int, T> setAction) {
            if (!IsValid) {
                return;
            }

            for (var i = 0; i < _instances.Length; i++) {
                setAction.Invoke(_instances[i], nameId, val);
            }
        }

        /// <summary>
        /// Materialから適切な方法で値を取得する
        /// </summary>
        private T GetValue<T>(int nameId, Func<MaterialInstance, int, T> getAction) {
            if (!IsValid) {
                return default;
            }

            return getAction.Invoke(_instances[0], nameId);
        }
    }
}