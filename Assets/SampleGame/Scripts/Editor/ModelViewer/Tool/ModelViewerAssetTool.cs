using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using GameFramework.Core.Editor;
using SampleGame.Infrastructure.ModelViewer;

namespace SampleGame.Editor {
    /// <summary>
    /// モデルビューア用のアセットツール
    /// </summary>
    public class ModelViewerAssetTool {
        [MenuItem("Assets/Sample Game/Model Viewer/Create Model Viewer Asset")]
        public static void CreateSelectedModelViewerAssets() {
            var reset = !EditorUtility.DisplayDialog("確認", "既存の設定ファイルがある場合上書きしますか？", "スキップする", "上書きする");
            EditorTool.SetupSelectionAssets<GameObject>("pfb_", asset => CreateModelViewerAsset(asset, reset));
        }

        /// <summary>
        /// Model用のViewerAssetの作成
        /// </summary>
        public static void CreateModelViewerAsset(GameObject asset, bool reset) {
            var prefabType = PrefabUtility.GetPrefabAssetType(asset);
            if (prefabType != PrefabAssetType.Variant && prefabType != PrefabAssetType.Regular) {
                return;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            var directoryPath = Path.GetDirectoryName(path) ?? "";
            var assetName = Path.GetFileNameWithoutExtension(path);

            if (!path.Contains("/Actor/")) {
                return;
            }

            var assetKey = assetName.Substring("pfb_".Length);

            // Viewer用のSetupDataを作成
            var setupDataPath = new ModelViewerActorMasterDataRequest(assetKey).Address;

            // 既にある場合は何もしない
            var destAsset = AssetDatabase.LoadAssetAtPath<ModelViewerActorMasterData>(setupDataPath);
            if (!reset && destAsset != null) {
                return;
            }

            var setupData = ScriptableObject.CreateInstance<ModelViewerActorMasterData>();
            setupData.name = $"dat_model_viewer_actor_master_{assetKey}";
            setupData.prefab = asset;

            // モーションを探す
            var boneKey = assetKey.Substring(0, "xx000".Length);
            var animationClipGuids = AssetDatabase.FindAssets($"t:{nameof(AnimationClip)} anim_{boneKey}_");
            var animationClips = new List<AnimationClip>();
            foreach (var guid in animationClipGuids) {
                var clipPath = AssetDatabase.GUIDToAssetPath(guid);
                var loadedObjects = AssetDatabase.LoadAllAssetsAtPath(clipPath);
                foreach (var loadedObj in loadedObjects) {
                    if (loadedObj is not AnimationClip) {
                        continue;
                    }

                    if (!loadedObj.name.StartsWith("anim_")) {
                        continue;
                    }

                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(loadedObj, out var id, out long _)) {
                        continue;
                    }

                    if (guid == id) {
                        animationClips.Add((AnimationClip)loadedObj);
                        break;
                    }
                }
            }

            setupData.animationClips = animationClips.ToArray();

            // Idleっぽいものを探す
            var idleIndex = animationClips.FindIndex(x => x.name.Contains("_idle") && x.isLooping);
            setupData.defaultAnimationClipIndex = idleIndex;

            // アバターパーツを探す
            var partsPath = Path.Combine(directoryPath, "Parts");
            if (Directory.Exists(partsPath)) {
                var avatarDict = new Dictionary<string, List<GameObject>>();
                var partPaths = Directory.GetDirectories(partsPath);
                foreach (var partPath in partPaths) {
                    var partDirectory = new DirectoryInfo(partPath);
                    var key = partDirectory.Name.ToLower();
                    if (!avatarDict.TryGetValue(key, out var list)) {
                        list = new();
                        avatarDict[key] = list;
                    }

                    // 含まれているPrefabをリストアップ
                    var foundGuids = AssetDatabase.FindAssets($"t:prefab pfb_{boneKey}", new[] { partPath });
                    foreach (var guid in foundGuids) {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                        list.Add(prefab);
                    }
                }
                
                // 設定に追加
                setupData.meshAvatarInfos = avatarDict
                    .Select(x => new ModelViewerActorMasterData.MeshAvatarInfo {
                        key = x.Key,
                        prefabs = x.Value.ToArray(),
                        defaultIndex = 0
                    }).ToArray();
            }

            // 元ファイルがある場合は、上書きコピー
            if (destAsset != null) {
                EditorUtility.CopySerialized(setupData, destAsset);
            }
            // 新規作成
            else {
                AssetDatabase.CreateAsset(setupData, setupDataPath);
            }
        }
    }
}