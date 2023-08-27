using UnityEditor;
using UnityEngine;

namespace SampleGame.Editor {
    /// <summary>
    /// Asset初期化用ツール
    /// </summary>
    partial class AssetSetupTool {
        /// <summary>
        /// 選択中Motionの初期化
        /// </summary>
        [MenuItem("Assets/SampleGame/Asset Setup/Motion")]
        public static void SetupSelectionMotionAssets() {
            SetupSelectionAssets<GameObject>(nameof(SetupSelectionMotionAssets), SetupMotionAsset);
        }

        /// <summary>
        /// モーションアセットの初期化
        /// </summary>
        private static void SetupMotionAsset(GameObject asset) {
            var type = PrefabUtility.GetPrefabAssetType(asset);
            if (type != PrefabAssetType.Model) {
                return;
            }

            var path = AssetDatabase.GetAssetPath(asset);

            // Importerの取得
            var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
            if (modelImporter == null) {
                return;
            }

            // TakeNameをFBXの名前とそろえる
            var baseName = asset.name;
            var clipAnimations = modelImporter.clipAnimations;
            var singleClip = clipAnimations.Length == 1;
            for (var i = 0; i < clipAnimations.Length; i++) {
                var clipAnimation = clipAnimations[i];
                var suffix = singleClip ? "" : $"_{i:00}";
                clipAnimation.takeName = baseName + suffix;
                clipAnimation.name = clipAnimation.takeName;
            }

            // Materialは全てOFF
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;

            // Meshの設定も基本削除
            modelImporter.meshCompression = ModelImporterMeshCompression.Off;
            modelImporter.meshOptimizationFlags = 0;
            modelImporter.optimizeMeshPolygons = false;
            modelImporter.optimizeMeshVertices = false;
            modelImporter.importNormals = ModelImporterNormals.None;

            // Avatarの設定
            modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            modelImporter.clipAnimations = clipAnimations;
            modelImporter.SaveAndReimport();
        }
    }
}