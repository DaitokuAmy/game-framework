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
        /// 選択中Motionの初期化
        /// </summary>
        [MenuItem("Assets/Third Person Engine/Asset Setup/Motion")]
        public static void SetupSelectionMotionAssets() {
            var reset = !EditorUtility.DisplayDialog("確認", "AnimationClipを廃棄して再生成し直しますか？", "維持する", "廃棄する");
            EditorTool.SetupSelectionAssets<GameObject>(nameof(SetupSelectionMotionAssets), asset => SetupMotionAsset(asset, reset));
        }

        /// <summary>
        /// モーションアセットの初期化
        /// </summary>
        private static void SetupMotionAsset(GameObject asset, bool reset) {
            var type = PrefabUtility.GetPrefabAssetType(asset);
            if (type != PrefabAssetType.Model) {
                return;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            var assetName = Path.GetFileNameWithoutExtension(path);

            if (!path.Contains("/Motion/") && !path.Contains("/Cutscene/")) {
                return;
            }

            // Importerの取得
            var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
            if (modelImporter == null) {
                return;
            }

            // 各種Import設定初期化
            modelImporter.importVisibility = false;
            modelImporter.importCameras = true;
            modelImporter.importLights = false;
            modelImporter.meshCompression = ModelImporterMeshCompression.Off;
            modelImporter.meshOptimizationFlags = 0;
            modelImporter.isReadable = false;
            modelImporter.optimizeMeshPolygons = false;
            modelImporter.optimizeMeshVertices = false;
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;

            // TakeNameをFBXの名前とそろえる
            var takeName = assetName;
            if (!takeName.StartsWith("anim_")) {
                takeName = $"anim_{takeName}";
            }

            var clipAnimations = reset ? modelImporter.defaultClipAnimations : modelImporter.clipAnimations;
            clipAnimations = clipAnimations.Length > 0 ? clipAnimations : modelImporter.defaultClipAnimations;
            var singleClip = clipAnimations.Length == 1;
            var poseClip = modelImporter.defaultClipAnimations.Length == 1 && assetName.EndsWith("_pose");
            if (poseClip) {
                var baseClipAnimation = modelImporter.defaultClipAnimations[0];
                var clipCount = ((int)baseClipAnimation.lastFrame - (int)baseClipAnimation.firstFrame) / 2;
                if (reset || clipAnimations.Length != clipCount) {
                    clipAnimations = new ModelImporterClipAnimation[clipCount];
                    for (var i = 0; i < clipAnimations.Length; i++) {
                        clipAnimations[i] = new ModelImporterClipAnimation();
                        clipAnimations[i].maskType = baseClipAnimation.maskType;
                        clipAnimations[i].maskSource = baseClipAnimation.maskSource;
                        clipAnimations[i].heightFromFeet = baseClipAnimation.heightFromFeet;
                        clipAnimations[i].heightOffset = baseClipAnimation.heightOffset;
                        clipAnimations[i].rotationOffset = baseClipAnimation.rotationOffset;
                        clipAnimations[i].lockRootRotation = true;
                        clipAnimations[i].lockRootHeightY = true;
                        clipAnimations[i].lockRootPositionXZ = true;
                        clipAnimations[i].keepOriginalOrientation = baseClipAnimation.keepOriginalOrientation;
                        clipAnimations[i].keepOriginalPositionY = baseClipAnimation.keepOriginalPositionY;
                        clipAnimations[i].keepOriginalPositionXZ = baseClipAnimation.keepOriginalPositionXZ;
                        clipAnimations[i].additiveReferencePoseFrame = baseClipAnimation.additiveReferencePoseFrame;
                        clipAnimations[i].hasAdditiveReferencePose = baseClipAnimation.hasAdditiveReferencePose;
                    }
                }

                var rawTakeName = takeName.Substring(0, takeName.Length - "_pose".Length);
                for (var i = 0; i < clipAnimations.Length; i++) {
                    clipAnimations[i].name = $"{rawTakeName}_{i:00}_lp";
                    clipAnimations[i].takeName = clipAnimations[i].name;
                    clipAnimations[i].firstFrame = i * 2 + 1;
                    clipAnimations[i].lastFrame = clipAnimations[i].firstFrame + 0.1f;
                    clipAnimations[i].loop = true;
                    clipAnimations[i].loopTime = true;
                    clipAnimations[i].loopPose = true;
                }
            }
            else {
                for (var i = 0; i < clipAnimations.Length; i++) {
                    var clipAnimation = clipAnimations[i];
                    if (singleClip) {
                        clipAnimation.firstFrame = Mathf.Max(0.0f, modelImporter.defaultClipAnimations[0].firstFrame);
                        clipAnimation.lastFrame = modelImporter.defaultClipAnimations[0].lastFrame;
                        clipAnimation.name = $"{takeName}";
                        clipAnimation.takeName = clipAnimation.name;
                        if (reset) {
                            clipAnimation.lockRootRotation = true;
                            clipAnimation.lockRootHeightY = true;
                            clipAnimation.lockRootPositionXZ = takeName.Contains("idle");
                            clipAnimation.keepOriginalOrientation = true;
                            clipAnimation.keepOriginalPositionY = true;
                            clipAnimation.keepOriginalPositionXZ = true;
                        }
                    }
                    else {
                        clipAnimation.name = $"{takeName}_{i:00}{(clipAnimation.loop ? "_lp" : "")}";
                        clipAnimation.takeName = clipAnimation.name;
                    }

                    if (clipAnimation.name.EndsWith("_lp")) {
                        clipAnimation.loop = true;
                        clipAnimation.loopTime = true;
                        clipAnimation.loopPose = true;
                    }
                }
            }

            modelImporter.clipAnimations = clipAnimations;
            modelImporter.SaveAndReimport();
        }
    }
}