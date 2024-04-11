using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.RendererSystems {
    /// <summary>
    /// マテリアル制御用インスタンス
    /// </summary>
    public class MaterialInstance : IDisposable {
#if UNITY_EDITOR
        /// <summary>
        /// Editorモード用のRendererキャッシュ
        /// </summary>
        private static class RendererEditorCache {
            private class CacheInfo {
                public int referenceCount;
                public List<Material> materials;
            }

            private static readonly Dictionary<Renderer, CacheInfo> s_cacheInfos = new();

            /// <summary>
            /// CloneMaterialの取得
            /// </summary>
            public static void GetCloneMaterials(Renderer renderer, List<Material> materials) {
                if (renderer == null) {
                    return;
                }

                if (!s_cacheInfos.TryGetValue(renderer, out var cacheInfo)) {
                    renderer.GetSharedMaterials(materials);
                    for (var i = 0; i < materials.Count; i++) {
                        materials[i] = Object.Instantiate(materials[i]);
                    }

                    s_cacheInfos[renderer] = new CacheInfo {
                        referenceCount = 1,
                        materials = materials
                    };
                    renderer.SetSharedMaterials(materials);
                }
                else {
                    materials.Clear();
                    materials.AddRange(cacheInfo.materials);
                    cacheInfo.referenceCount++;
                }
            }

            /// <summary>
            /// CloneMaterialの返却
            /// </summary>
            public static void ReturnCloneMaterials(Renderer renderer) {
                if (renderer == null) {
                    return;
                }

                if (s_cacheInfos.TryGetValue(renderer, out var cacheInfo)) {
                    cacheInfo.referenceCount--;
                    if (cacheInfo.referenceCount <= 0) {
                        foreach (var material in cacheInfo.materials) {
                            Object.DestroyImmediate(material);
                        }

                        s_cacheInfos.Remove(renderer);
                    }
                }
            }
        }
#endif

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

        private bool _clonedMaterials;

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
                case ControlType.Clone: {
                    var materials = new List<Material>();
                    GetClonedMaterials(renderer, materials);
                    _clonedMaterial = materials[materialIndex];
                    break;
                }
                case ControlType.PropertyBlock:
                    _block = new MaterialPropertyBlock();
                    break;
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_clonedMaterials) {
#if UNITY_EDITOR
                RendererEditorCache.ReturnCloneMaterials(_renderer);
#endif
                _clonedMaterials = false;
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
            float GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetFloat(id);
            float GetMaterialAction(Material mat, int id) => mat.GetFloat(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public float GetFloat(string propertyName) => GetFloat(Shader.PropertyToID(propertyName));

        public int GetInt(int nameId) {
            int GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetInt(id);
            int GetMaterialAction(Material mat, int id) => mat.GetInt(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public int GetInt(string propertyName) => GetInt(Shader.PropertyToID(propertyName));

        public Vector4 GetVector(int nameId) {
            Vector4 GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetVector(id);
            Vector4 GetMaterialAction(Material mat, int id) => mat.GetVector(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public Vector4 GetVector(string propertyName) => GetVector(Shader.PropertyToID(propertyName));

        public Color GetColor(int nameId) {
            Color GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetColor(id);
            Color GetMaterialAction(Material mat, int id) => mat.GetColor(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public Color GetColor(string propertyName) => GetColor(Shader.PropertyToID(propertyName));

        public Matrix4x4 GetMatrix(int nameId) {
            Matrix4x4 GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetMatrix(id);
            Matrix4x4 GetMaterialAction(Material mat, int id) => mat.GetMatrix(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public Matrix4x4 GetMatrix(string propertyName) => GetMatrix(Shader.PropertyToID(propertyName));

        public float[] GetFloatArray(int nameId) {
            float[] GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetFloatArray(id);
            float[] GetMaterialAction(Material mat, int id) => mat.GetFloatArray(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public float[] GetFloatArray(string propertyName) => GetFloatArray(Shader.PropertyToID(propertyName));

        public Vector4[] GetVectorArray(int nameId) {
            Vector4[] GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetVectorArray(id);
            Vector4[] GetMaterialAction(Material mat, int id) => mat.GetVectorArray(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public Vector4[] GetVectorArray(string propertyName) => GetVectorArray(Shader.PropertyToID(propertyName));

        public Matrix4x4[] GetMatrixArray(int nameId) {
            Matrix4x4[] GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetMatrixArray(id);
            Matrix4x4[] GetMaterialAction(Material mat, int id) => mat.GetMatrixArray(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public Matrix4x4[] GetMatrixArray(string propertyName) => GetMatrixArray(Shader.PropertyToID(propertyName));

        public Texture GetTexture(int nameId) {
            Texture GetPropertyAction(MaterialPropertyBlock block, int id) => block.GetTexture(id);
            Texture GetMaterialAction(Material mat, int id) => mat.GetTexture(id);

            return GetValue(nameId, GetPropertyAction, GetMaterialAction);
        }

        public Texture GetTexture(string propertyName) => GetTexture(Shader.PropertyToID(propertyName));

        /// <summary>
        /// 各種セッター
        /// </summary>
        public void SetFloat(int nameId, float val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, float v) => block.SetFloat(id, v);
            void SetMaterialAction(Material mat, int id, float v) => mat.SetFloat(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
        }

        public void SetFloat(string propertyName, float val) => SetFloat(Shader.PropertyToID(propertyName), val);

        public void SetInt(int nameId, int val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, int v) => block.SetInt(id, v);
            void SetMaterialAction(Material mat, int id, int v) => mat.SetInt(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
        }

        public void SetInt(string propertyName, int val) => SetInt(Shader.PropertyToID(propertyName), val);

        public void SetVector(int nameId, Vector4 val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, Vector4 v) => block.SetVector(id, v);
            void SetMaterialAction(Material mat, int id, Vector4 v) => mat.SetVector(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
        }

        public void SetVector(string propertyName, Vector4 val) => SetVector(Shader.PropertyToID(propertyName), val);

        public void SetColor(int nameId, Color val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, Color v) => block.SetColor(id, v);
            void SetMaterialAction(Material mat, int id, Color v) => mat.SetColor(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
        }

        public void SetColor(string propertyName, Color val) => SetColor(Shader.PropertyToID(propertyName), val);

        public void SetMatrix(int nameId, Matrix4x4 val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, Matrix4x4 v) => block.SetMatrix(id, v);
            void SetMaterialAction(Material mat, int id, Matrix4x4 v) => mat.SetMatrix(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
        }

        public void SetMatrix(string propertyName, Matrix4x4 val) => SetMatrix(Shader.PropertyToID(propertyName), val);

        public void SetFloatArray(int nameId, float[] val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, float[] v) => block.SetFloatArray(id, v);
            void SetMaterialAction(Material mat, int id, float[] v) => mat.SetFloatArray(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
        }

        public void SetFloatArray(string propertyName, float[] val) => SetFloatArray(Shader.PropertyToID(propertyName), val);

        public void SetVectorArray(int nameId, Vector4[] val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, Vector4[] v) => block.SetVectorArray(id, v);
            void SetMaterialAction(Material mat, int id, Vector4[] v) => mat.SetVectorArray(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
        }

        public void SetVectorArray(string propertyName, Vector4[] val) => SetVectorArray(Shader.PropertyToID(propertyName), val);

        public void SetMatrixArray(int nameId, Matrix4x4[] val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, Matrix4x4[] v) => block.SetMatrixArray(id, v);
            void SetMaterialAction(Material mat, int id, Matrix4x4[] v) => mat.SetMatrixArray(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
        }

        public void SetMatrixArray(string propertyName, Matrix4x4[] val) => SetMatrixArray(Shader.PropertyToID(propertyName), val);

        public void SetTexture(int nameId, Texture val) {
            void SetPropertyAction(MaterialPropertyBlock block, int id, Texture v) => block.SetTexture(id, v);
            void SetMaterialAction(Material mat, int id, Texture v) => mat.SetTexture(id, v);

            SetValue(nameId, val, SetPropertyAction, SetMaterialAction);
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
                return getMaterialAction.Invoke(_clonedMaterial, nameId);
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

        /// <summary>
        /// クローン用のマテリアル取得
        /// </summary>
        private void GetClonedMaterials(Renderer renderer, List<Material> materials) {
            // 再生時は通常の取得フロー
            if (Application.isPlaying) {
                renderer.GetMaterials(materials);
            }
#if UNITY_EDITOR
            // 非再生時は独自でキャッシュする（エラーになるので）
            else {
                RendererEditorCache.GetCloneMaterials(renderer, materials);
                _clonedMaterials = true;
            }
#endif
        }
    }
}