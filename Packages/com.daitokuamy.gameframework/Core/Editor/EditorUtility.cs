using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Core.Editor {
    /// <summary>
    /// エディタ用のユーティリティ
    /// </summary>
    public static partial class EditorUtility {
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
                    UnityEditor.EditorUtility.DisplayProgressBar(title, assetPath, progress);
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
                UnityEditor.EditorUtility.ClearProgressBar();
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
                    UnityEditor.EditorUtility.DisplayProgressBar(title, assetPath, progress);
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
                UnityEditor.EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// TransformPathの取得
        /// </summary>
        public static string GetTransformPath(Transform root, Transform child, string path = null) {
            if (root == child) {
                return path ?? "";
            }

            return GetTransformPath(root, child.parent, path != null ? $"{child.name}/{path}" : child.name);
        }

        /// <summary>
        /// Prefabの編集
        /// ※Prefabは直接編集せずにこの関数を通して編集してください
        /// </summary>
        /// <param name="prefab">編集対象のPrefab</param>
        /// <param name="editAction">編集処理</param>
        public static void EditPrefab(GameObject prefab, Action<GameObject> editAction) {
            var assetPath = AssetDatabase.GetAssetPath(prefab);

            // Prefabを展開
            var contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);

            // 編集処理
            editAction(contentsRoot);

            // Prefabの保存
            PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(contentsRoot);
        }

        /// <summary>
        /// CopyablePropertyアトリビュートがついたプロパティをコピーする
        /// todo:未実装
        /// </summary>
        public static void CopySerializedObjectForAttribute(Object source, Object dest) {
            var sourceObj = new SerializedObject(source);
            var destObj = new SerializedObject(dest);
            var sourceType = source.GetType();

            destObj.Update();

            var itr = sourceObj.GetIterator();
            while (itr.NextVisible(true)) {
                var type = sourceType;
                var field = type.GetField(itr.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                while (field == null || field.GetCustomAttribute<CopyablePropertyAttribute>() == null) {
                    type = type.BaseType;
                    if (type == null || type == typeof(Object)) {
                        field = null;
                        break;
                    }

                    field = type.GetField(itr.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                }

                if (field == null) {
                    continue;
                }

                //var sourceProperty = sourceObj.FindProperty(itr.propertyPath);
                //destObj.CopyFromSerializedPropertyIfDifferent(sourceProperty);
                Debug.Log($"Test:{itr.propertyPath}");
            }

            destObj.ApplyModifiedProperties();
        }
    }
}