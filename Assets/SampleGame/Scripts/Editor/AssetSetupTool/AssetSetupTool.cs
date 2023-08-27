using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SampleGame.Editor {
    /// <summary>
    /// Asset初期化用ツール
    /// </summary>
    public static partial class AssetSetupTool {
        /// <summary>
        /// Projectに含まれるAssetをフィルターして初期化
        /// </summary>
        /// <param name="title">ProgressBarのタイトル</param>
        /// <param name="filter">Asset検索用フィルター</param>
        /// <param name="searchInFolders">検索対象フォルダリスト</param>
        /// <param name="setupAction">初期化処理</param>
        public static void SetupFilteredAssets<T>(string title, string filter, string[] searchInFolders, Action<T> setupAction)
            where T : Object {
            filter = $"t:{typeof(T).Name} {filter}";
            var guids = default(string[]);
            if (searchInFolders == null || searchInFolders.Length <= 0) {
                guids = AssetDatabase.FindAssets(filter);
            }
            else {
                guids = AssetDatabase.FindAssets(filter, searchInFolders);
            }

            try {
                for (var i = 0; i < guids.Length; i++) {
                    var progress = i / (float)guids.Length;
                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    EditorUtility.DisplayProgressBar(title, assetPath, progress);
                    try {
                        var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                        setupAction.Invoke(asset);
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Projectに含まれるAssetをフィルターして初期化
        /// </summary>
        /// <param name="title">ProgressBarのタイトル</param>
        /// <param name="filter">Asset検索用フィルター</param>
        /// <param name="setupAction">初期化処理</param>
        public static void SetupFilteredAssets<T>(string title, string filter, Action<T> setupAction)
            where T : Object {
            SetupFilteredAssets(title, filter, null, setupAction);
        }

        /// <summary>
        /// 選択中のアセットを初期化
        /// </summary>
        /// <param name="title">ProgressBarのタイトル</param>
        /// <param name="setupAction">初期化処理</param>
        public static void SetupSelectionAssets<T>(string title, Action<T> setupAction)
            where T : Object {
            var guids = Selection.assetGUIDs
                .Where(x => AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GUIDToAssetPath(x)) == typeof(T))
                .ToList();
            var folderPaths = Selection.GetFiltered<DefaultAsset>(SelectionMode.TopLevel)
                .Select(x => AssetDatabase.GetAssetPath(x))
                .ToArray();
            if (folderPaths.Length > 0) {
                guids.AddRange(AssetDatabase.FindAssets($"t:{typeof(T).Name}", folderPaths));
            }

            try {
                for (var i = 0; i < guids.Count; i++) {
                    var guid = guids[i];
                    var progress = i / (float)guids.Count;
                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    EditorUtility.DisplayProgressBar(title, assetPath, progress);
                    try {
                        var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                        setupAction.Invoke(asset);
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Prefabの編集
        /// </summary>
        /// <param name="prefab">編集対象のPrefab</param>
        /// <param name="editAction">編集処理</param>
        public static void EditPrefab(GameObject prefab, Action<GameObject> editAction) {
            var path = AssetDatabase.GetAssetPath(prefab);
            var instance = PrefabUtility.LoadPrefabContents(path);

            editAction.Invoke(instance);

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }

        /// <summary>
        /// Materialの自動展開
        /// </summary>
        public static void ExtractMaterials(ModelImporter importer, string destinationRelativePath) {
            var reloadAssetPaths = new HashSet<string>();
            var materials = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath)
                .Where(x => x.GetType() == typeof(Material))
                .ToArray();

            var destinationPath = Path.GetDirectoryName(importer.assetPath) ?? "";
            destinationPath = Path.Combine(destinationPath, destinationRelativePath);
            if (!Directory.Exists(destinationPath)) {
                Directory.CreateDirectory(destinationPath);
            }

            var externalObjectMap = importer.GetExternalObjectMap();
            foreach (var pair in externalObjectMap) {
                if (pair.Value == null) {
                    importer.RemoveRemap(pair.Key);
                }
            }

            foreach (var material in materials) {
                var newAssetPath = Path.Combine(destinationPath, material.name) + ".mat";
                var existsAsset = AssetDatabase.LoadAssetAtPath<Material>(newAssetPath);
                if (existsAsset != null) {
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(material), existsAsset);
                }
                else {
                    newAssetPath = AssetDatabase.GenerateUniqueAssetPath(newAssetPath);

                    var error = AssetDatabase.ExtractAsset(material, newAssetPath);
                    if (!string.IsNullOrEmpty(error)) {
                        continue;
                    }
                }

                reloadAssetPaths.Add(importer.assetPath);
            }

            foreach (var path in reloadAssetPaths) {
                AssetDatabase.WriteImportSettingsIfDirty(path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}