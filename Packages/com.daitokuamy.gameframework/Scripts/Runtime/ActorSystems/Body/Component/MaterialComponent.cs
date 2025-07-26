using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Body用のMaterial制御クラス
    /// </summary>
    public class MaterialComponent : BodyComponent {
        // キャッシュ用のMaterial情報リスト
        private readonly Dictionary<string, List<MaterialInstance>> _materialInfos = new();

        /// <summary>Material情報のリフレッシュ</summary>
        public event Action RefreshedEvent;

        /// <summary>
        /// 制御キーの一覧を取得
        /// </summary>
        public string[] GetKeys() {
            return _materialInfos.Keys.ToArray();
        }

        /// <summary>
        /// マテリアル制御情報の取得
        /// </summary>
        /// <param name="key">制御用キー</param>
        public MaterialHandle GetHandle(string key) {
            if (_materialInfos.TryGetValue(key, out var result)) {
                return new MaterialHandle(result);
            }

            return default;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            var meshController = Body.GetBodyComponent<MeshComponent>();
            meshController.RefreshedEvent += () => {
                // Material情報の回収
                CreateMaterialInfos();
            };
            CreateMaterialInfos();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            DestroyMaterialInfos();
        }

        /// <summary>
        /// マテリアル情報の生成
        /// </summary>
        private void CreateMaterialInfos() {
            DestroyMaterialInfos();

            var partsList = Body.GetComponentsInChildren<MaterialParts>(true);
            for (var i = 0; i < partsList.Length; i++) {
                var parts = partsList[i];
                for (var j = 0; j < parts.infos.Length; j++) {
                    var info = parts.infos[j];

                    if (!_materialInfos.TryGetValue(info.key, out var list)) {
                        list = new List<MaterialInstance>();
                        _materialInfos.Add(info.key, list);
                    }

                    for (var k = 0; k < info.rendererMaterials.Length; k++) {
                        var rendererMaterial = info.rendererMaterials[k];
                        if (!rendererMaterial.IsValid) {
                            continue;
                        }
                        
                        var materialInfo = new MaterialInstance(rendererMaterial.renderer, rendererMaterial.materialIndex, info.controlType);
                        list.Add(materialInfo);
                    }
                }
            }

            RefreshedEvent?.Invoke();
        }

        /// <summary>
        /// マテリアル情報の削除
        /// </summary>
        private void DestroyMaterialInfos() {
            foreach (var pair in _materialInfos) {
                foreach (var instance in pair.Value) {
                    instance.Dispose();
                }
            }
            
            _materialInfos.Clear();
        }
    }
}