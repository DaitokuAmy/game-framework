using System.IO;
using UnityEditor;
using UnityEngine;

namespace SampleGame.Editor {
    /// <summary>
    /// Asset初期化用ツール
    /// </summary>
    partial class AssetSetupTool {
        /// <summary>
        /// 選択中Modelの初期化
        /// </summary>
        [MenuItem("Assets/SampleGame/Asset Setup/Model")]
        public static void SetupSelectionModelAssets() {
            SetupSelectionAssets<GameObject>(nameof(SetupSelectionModelAssets), SetupModelAsset);
        }

        /// <summary>
        /// VariantPrefabの生成
        /// </summary>
        [MenuItem("Assets/SampleGame/Asset Setup/Create Variant")]
        public static void CreateVariantPrefabs() {
            SetupSelectionAssets<GameObject>(nameof(CreateVariantPrefabs), CreateVariantPrefab);
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
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.importCameras = false;
            importer.importConstraints = false;
            importer.importLights = false;
            importer.importBlendShapes = true;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            ExtractMaterials(importer, "Materials");
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

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            var path = AssetDatabase.GetAssetPath(asset);
            var directoryName = Path.GetDirectoryName(path) ?? "Assets";
            directoryName = Path.Combine(directoryName, "Prefabs");
            if (!Directory.Exists(directoryName)) {
                Directory.CreateDirectory(directoryName);
            }
            var fileName = Path.GetFileNameWithoutExtension(path);
            fileName = "prfb_" + fileName.Replace("stat_", "").Replace("skin_", "") + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, Path.Combine(directoryName, fileName));
            Object.DestroyImmediate(instance);
        }
    }
}