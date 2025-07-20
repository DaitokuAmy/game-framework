using System.IO;
using GameFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace ThirdPersonEngine.Editor {
    /// <summary>
    /// Asset初期化用ツール
    /// </summary>
    partial class AssetSetupTool {
        /// <summary>
        /// 選択中Modelの初期化
        /// </summary>
        [MenuItem("Assets/Third Person Engine/Asset Setup/Model")]
        public static void SetupSelectionModelAssets() {
            EditorTool.SetupSelectionAssets<GameObject>(nameof(SetupSelectionModelAssets), SetupModelAsset);
        }

        /// <summary>
        /// VariantPrefabの生成
        /// </summary>
        [MenuItem("Assets/Third Person Engine/Asset Setup/Create Variant")]
        public static void CreateVariantPrefabs() {
            EditorTool.SetupSelectionAssets<GameObject>(nameof(CreateVariantPrefabs), CreateVariantPrefab);
        }

        /// <summary>
        /// モデルアセットの初期化
        /// </summary>
        private static void SetupModelAsset(GameObject asset) {
            var prefabType = PrefabUtility.GetPrefabAssetType(asset);
            if (prefabType != PrefabAssetType.Model) {
                return;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            if (!path.Contains("/Body/") && !path.Contains("/Vfx/")) {
                return;
            }
            
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.importCameras = false;
            importer.importConstraints = false;
            importer.importLights = false;
            importer.importBlendShapes = true;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            EditorTool.ExtractMaterials(importer, "../Materials");
            importer.SaveAndReimport();
            
            EditorUtility.SetDirty(asset);
        }

        /// <summary>
        /// VariantPrefabの生成
        /// </summary>
        private static void CreateVariantPrefab(GameObject asset) {
            var prefabType = PrefabUtility.GetPrefabAssetType(asset);
            if (prefabType != PrefabAssetType.Model) {
                return;
            }

            var obj = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            try {
                var path = AssetDatabase.GetAssetPath(asset);
                var prefabPath = path.Replace("mdl_", "pfb_")
                    .Replace(".fbx", ".prefab")
                    .Replace("/Meshes/", "/");

                var fileName = Path.GetFileName(prefabPath);
                if (!fileName.StartsWith("pfb_")) {
                    prefabPath = prefabPath.Replace(fileName, $"pfb_{fileName}");
                }

                PrefabUtility.SaveAsPrefabAssetAndConnect(obj, prefabPath, InteractionMode.AutomatedAction);
            }
            finally {
                Object.DestroyImmediate(obj);
            }
        }
    }
}