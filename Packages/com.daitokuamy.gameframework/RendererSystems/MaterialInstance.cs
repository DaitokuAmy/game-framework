using System;
using UnityEngine;

namespace GameFramework.RendererSystems {
    /// <summary>
    /// マテリアル制御用インスタンス
    /// </summary>
    public class MaterialInstance {
        /// <summary>
        /// 制御タイプ
        /// </summary>
        public enum ControlType {
            // Editor:PropertyBlock, Runtime:Clone
            Auto = -1,
            // Materialをそのまま扱う(.sharedMaterial)
            Raw,
            // Materialをコピーして使う(.material)
            Clone,
            // MaterialPropertyBlockを使う
            PropertyBlock,
        }

        private readonly Renderer _renderer;
        private readonly int _materialIndex;
        private readonly Material _material;
        private readonly Material _clonedMaterial;
        private readonly MaterialPropertyBlock _block;

        /// <summary>Materialを保持しているRenderer</summary>
        public Renderer Renderer => _renderer;
        /// <summary>元のMaterial</summary>
        public Material Material => _material;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="renderer">Materialを保持するRenderer</param>
        /// <param name="materialIndex">MaterialのIndex</param>
        /// <param name="controlType">制御タイプ</param>
        public MaterialInstance(Renderer renderer, int materialIndex, ControlType controlType) {
            _renderer = renderer;
            _materialIndex = materialIndex;
            _material = renderer.sharedMaterials[materialIndex];

            if (controlType == ControlType.Auto) {
#if UNITY_EDITOR
                controlType = ControlType.PropertyBlock;
#else
                controlType = ControlType.Clone;
#endif
            }
            
#if DISABLE_MATERIAL_PROPERTY_BLOCK
            if (controlType == ControlType.PropertyBlock) {
                controlType = ControlType.Clone;
            }
#endif
            
            switch (controlType) {
                case ControlType.Clone:
                    _clonedMaterial = renderer.materials[materialIndex];
                    break;
                case ControlType.PropertyBlock:
                    _block = new MaterialPropertyBlock();
                    break;
            }
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public override string ToString() {
            if (_renderer == null || _material == null) {
                return "Invalid Instance";
            }

            return $"{_renderer.name}[{_materialIndex}]:{_material.name}";
        }

        /// <summary>
        /// 各種ゲッター
        /// </summary>
        public float GetFloat(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetFloat(n),
                (m, n) => m.GetFloat(n));
        }

        public float GetFloat(string propertyName) => GetFloat(Shader.PropertyToID(propertyName));

        public int GetInt(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetInt(n),
                (m, n) => m.GetInt(n));
        }

        public int GetInt(string propertyName) => GetInt(Shader.PropertyToID(propertyName));

        public Vector4 GetVector(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetVector(n),
                (m, n) => m.GetVector(n));
        }

        public Vector4 GetVector(string propertyName) => GetVector(Shader.PropertyToID(propertyName));

        public Color GetColor(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetColor(n),
                (m, n) => m.GetColor(n));
        }

        public Color GetColor(string propertyName) => GetColor(Shader.PropertyToID(propertyName));

        public Matrix4x4 GetMatrix(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetMatrix(n),
                (m, n) => m.GetMatrix(n));
        }

        public Matrix4x4 GetMatrix(string propertyName) => GetMatrix(Shader.PropertyToID(propertyName));

        public float[] GetFloatArray(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetFloatArray(n),
                (m, n) => m.GetFloatArray(n));
        }

        public float[] GetFloatArray(string propertyName) => GetFloatArray(Shader.PropertyToID(propertyName));

        public Vector4[] GetVectorArray(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetVectorArray(n),
                (m, n) => m.GetVectorArray(n));
        }

        public Vector4[] GetVectorArray(string propertyName) => GetVectorArray(Shader.PropertyToID(propertyName));

        public Matrix4x4[] GetMatrixArray(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetMatrixArray(n),
                (m, n) => m.GetMatrixArray(n));
        }

        public Matrix4x4[] GetMatrixArray(string propertyName) => GetMatrixArray(Shader.PropertyToID(propertyName));

        public Texture GetTexture(int nameId) {
            return GetValue(nameId,
                (b, n) => b.GetTexture(n),
                (m, n) => m.GetTexture(n));
        }

        public Texture GetTexture(string propertyName) => GetTexture(Shader.PropertyToID(propertyName));

        /// <summary>
        /// 各種セッター
        /// </summary>
        public void SetFloat(int nameId, float val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetFloat(n, v),
                (m, n, v) => m.SetFloat(n, v));
        }

        public void SetFloat(string propertyName, float val) => SetFloat(Shader.PropertyToID(propertyName), val);

        public void SetInt(int nameId, int val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetInt(n, v),
                (m, n, v) => m.SetInt(n, v));
        }

        public void SetInt(string propertyName, int val) => SetInt(Shader.PropertyToID(propertyName), val);

        public void SetVector(int nameId, Vector4 val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetVector(n, v),
                (m, n, v) => m.SetVector(n, v));
        }

        public void SetVector(string propertyName, Vector4 val) => SetVector(Shader.PropertyToID(propertyName), val);

        public void SetColor(int nameId, Color val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetColor(n, v),
                (m, n, v) => m.SetColor(n, v));
        }

        public void SetColor(string propertyName, Color val) => SetColor(Shader.PropertyToID(propertyName), val);

        public void SetMatrix(int nameId, Matrix4x4 val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetMatrix(n, v),
                (m, n, v) => m.SetMatrix(n, v));
        }

        public void SetMatrix(string propertyName, Matrix4x4 val) => SetMatrix(Shader.PropertyToID(propertyName), val);

        public void SetFloatArray(int nameId, float[] val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetFloatArray(n, v),
                (m, n, v) => m.SetFloatArray(n, v));
        }

        public void SetFloatArray(string propertyName, float[] val) => SetFloatArray(Shader.PropertyToID(propertyName), val);

        public void SetVectorArray(int nameId, Vector4[] val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetVectorArray(n, v),
                (m, n, v) => m.SetVectorArray(n, v));
        }

        public void SetVectorArray(string propertyName, Vector4[] val) => SetVectorArray(Shader.PropertyToID(propertyName), val);

        public void SetMatrixArray(int nameId, Matrix4x4[] val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetMatrixArray(n, v),
                (m, n, v) => m.SetMatrixArray(n, v));
        }

        public void SetMatrixArray(string propertyName, Matrix4x4[] val) => SetMatrixArray(Shader.PropertyToID(propertyName), val);

        public void SetTexture(int nameId, Texture val) {
            SetValue(nameId, val,
                (b, n, v) => b.SetTexture(n, v),
                (m, n, v) => m.SetTexture(n, v));
        }

        public void SetTexture(string propertyName, Texture val) => SetTexture(Shader.PropertyToID(propertyName), val);

        /// <summary>
        /// Materialから適切な方法で値を取得する
        /// </summary>
        private T GetValue<T>(int nameId,
            Func<MaterialPropertyBlock, int, T> getPropertyAction, Func<Material, int, T> getMaterialAction) {
            if (_block != null) {
                _renderer.GetPropertyBlock(_block, _materialIndex);
                return getPropertyAction.Invoke(_block, nameId);
            }

            if (_clonedMaterial != null) {
                getMaterialAction.Invoke(_clonedMaterial, nameId);
            }

            return getMaterialAction.Invoke(_material, nameId);
        }

        /// <summary>
        /// Materialに適切な方法で値を設定する
        /// </summary>
        private void SetValue<T>(int nameId, T val,
            Action<MaterialPropertyBlock, int, T> setPropertyAction, Action<Material, int, T> setMaterialAction) {
            if (_block != null) {
                _renderer.GetPropertyBlock(_block, _materialIndex);
                setPropertyAction.Invoke(_block, nameId, val);
                _renderer.SetPropertyBlock(_block, _materialIndex);
            }
            else if (_clonedMaterial != null) {
                setMaterialAction.Invoke(_clonedMaterial, nameId, val);
            }
            else {
                setMaterialAction.Invoke(_material, nameId, val);
            }
        }
    }
}