using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UISupportToolWindowの自動化モジュール
    /// </summary>
    internal sealed partial class UISupportToolWindow {
        /// <summary>
        /// UI作業の自動化機能モジュール
        /// </summary>
        private sealed class AutomationModule : EditorToolModule<UISupportToolWindow, UISupportUserData> {
            /// <summary>テンプレートフォルダ初期値パス</summary>
            private const string DefaultTemplatePrefabFolderPath = "Assets/Sample Game/UI/Prefabs/Template";

            private readonly List<GameObject> _templatePrefabs = new();
            private readonly List<string> _templateLabels = new();

            private TMP_FontAsset _targetFont;
            private Material _targetFontMaterial;
            private bool _isApplyFontMaterial = true;
            private int _selectedTemplateIndex;

            /// <summary>タブ表示名</summary>
            public override string DisplayName => "自動化";

            /// <summary>
            /// GUIを描画
            /// </summary>
            public override void OnGUI() {
                DrawNamingTools();
                EditorGUILayout.Space(8.0f);
                DrawFontReplaceTools();
                EditorGUILayout.Space(8.0f);
                DrawTemplateTools();
                EditorGUILayout.Space(8.0f);

                if (GUILayout.Button("選択対象のLayoutを再計算")) {
                    EditorSupportTool.RebuildLayoutsInSelection();
                }
            }

            /// <summary>
            /// 命名支援UIを描画
            /// </summary>
            private void DrawNamingTools() {
                EditorGUILayout.LabelField("命名", EditorStyles.boldLabel);
                Window.Data.RenameButtonSuffix = EditorGUILayout.TextField("Button接尾辞", Window.Data.RenameButtonSuffix);
                Window.Data.RenameTextSuffix = EditorGUILayout.TextField("Text接尾辞", Window.Data.RenameTextSuffix);
                Window.Data.RenameImageSuffix = EditorGUILayout.TextField("Image接尾辞", Window.Data.RenameImageSuffix);

                if (GUILayout.Button("選択UIを命名規則でリネーム")) {
                    RenameSelectedObjectsByConvention();
                }

                Window.SaveState();
            }

            /// <summary>
            /// フォント置換UIを描画
            /// </summary>
            private void DrawFontReplaceTools() {
                EditorGUILayout.LabelField("フォント", EditorStyles.boldLabel);
                _targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("置換先フォント", _targetFont, typeof(TMP_FontAsset), false);
                _targetFontMaterial = (Material)EditorGUILayout.ObjectField("置換先マテリアル", _targetFontMaterial, typeof(Material), false);
                _isApplyFontMaterial = EditorGUILayout.ToggleLeft("マテリアルも適用", _isApplyFontMaterial);

                if (GUILayout.Button("選択UIにフォント/マテリアルを適用")) {
                    EditorSupportTool.ReplaceFontInSelection(_targetFont, _targetFontMaterial, _isApplyFontMaterial);
                }
            }

            /// <summary>
            /// テンプレート追加UIを描画
            /// </summary>
            private void DrawTemplateTools() {
                EditorGUILayout.LabelField("テンプレート", EditorStyles.boldLabel);

                EnsureTemplateFolderGuidInitialized();
                DrawTemplateFolderField();

                using (new EditorGUILayout.HorizontalScope()) {
                    if (GUILayout.Button("テンプレート一覧を再読込")) {
                        ReloadTemplatePrefabs();
                    }

                    if (GUILayout.Button("選択テンプレートをシーンに追加")) {
                        CreateSelectedTemplate();
                    }
                }

                EnsureTemplatePrefabs();

                if (_templatePrefabs.Count <= 0) {
                    EditorGUILayout.HelpBox("テンプレートPrefabが見つかりません。", MessageType.Info);
                    return;
                }

                _selectedTemplateIndex = EditorGUILayout.Popup("追加テンプレート", _selectedTemplateIndex, _templateLabels.ToArray());
                _selectedTemplateIndex = Mathf.Clamp(_selectedTemplateIndex, 0, _templatePrefabs.Count - 1);
            }

            /// <summary>
            /// テンプレートフォルダ選択UIを描画
            /// </summary>
            private void DrawTemplateFolderField() {
                var folderPath = GetTemplateFolderPath();
                var folderAsset = string.IsNullOrEmpty(folderPath) ? null : AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);

                var selectedFolder = (DefaultAsset)EditorGUILayout.ObjectField("テンプレートフォルダ", folderAsset, typeof(DefaultAsset), false);
                if (selectedFolder == folderAsset) {
                    return;
                }

                var selectedPath = selectedFolder == null ? string.Empty : AssetDatabase.GetAssetPath(selectedFolder);
                if (!AssetDatabase.IsValidFolder(selectedPath)) {
                    selectedPath = string.Empty;
                }

                var guid = string.IsNullOrEmpty(selectedPath) ? string.Empty : AssetDatabase.AssetPathToGUID(selectedPath);
                Window.Data.TemplatePrefabFolderGuid = guid;
                Window.SaveState();

                _selectedTemplateIndex = 0;
                _templatePrefabs.Clear();
                _templateLabels.Clear();
            }

            /// <summary>
            /// テンプレート一覧の初期化を保証
            /// </summary>
            private void EnsureTemplatePrefabs() {
                if (_templatePrefabs.Count > 0 || _templateLabels.Count > 0) {
                    return;
                }

                ReloadTemplatePrefabs();
            }

            /// <summary>
            /// テンプレートフォルダGUIDの初期化を保証
            /// </summary>
            private void EnsureTemplateFolderGuidInitialized() {
                if (!string.IsNullOrEmpty(Window.Data.TemplatePrefabFolderGuid)) {
                    return;
                }

                if (!AssetDatabase.IsValidFolder(DefaultTemplatePrefabFolderPath)) {
                    return;
                }

                Window.Data.TemplatePrefabFolderGuid = AssetDatabase.AssetPathToGUID(DefaultTemplatePrefabFolderPath);
                Window.SaveState();
            }

            /// <summary>
            /// テンプレートフォルダのパスを取得
            /// </summary>
            private string GetTemplateFolderPath() {
                if (string.IsNullOrEmpty(Window.Data.TemplatePrefabFolderGuid)) {
                    return string.Empty;
                }

                return AssetDatabase.GUIDToAssetPath(Window.Data.TemplatePrefabFolderGuid);
            }

            /// <summary>
            /// テンプレートPrefab一覧を再読込
            /// </summary>
            private void ReloadTemplatePrefabs() {
                _templatePrefabs.Clear();
                _templateLabels.Clear();

                var folderPath = GetTemplateFolderPath();
                if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath)) {
                    _selectedTemplateIndex = 0;
                    return;
                }

                var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
                var paths = new List<string>(prefabGuids.Length);
                for (var i = 0; i < prefabGuids.Length; i++) {
                    paths.Add(AssetDatabase.GUIDToAssetPath(prefabGuids[i]));
                }

                paths.Sort(StringComparer.OrdinalIgnoreCase);

                for (var i = 0; i < paths.Count; i++) {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                    if (prefab == null) {
                        continue;
                    }

                    _templatePrefabs.Add(prefab);
                    _templateLabels.Add(prefab.name);
                }

                _selectedTemplateIndex = Mathf.Clamp(_selectedTemplateIndex, 0, Mathf.Max(0, _templatePrefabs.Count - 1));
            }

            /// <summary>
            /// 選択テンプレートをシーンへ追加
            /// </summary>
            private void CreateSelectedTemplate() {
                EnsureTemplatePrefabs();
                if (_templatePrefabs.Count <= 0) {
                    Debug.LogWarning("[UISupport] テンプレートPrefabが見つかりません。");
                    return;
                }

                var prefab = _templatePrefabs[Mathf.Clamp(_selectedTemplateIndex, 0, _templatePrefabs.Count - 1)];
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null) {
                    return;
                }

                Undo.RegisterCreatedObjectUndo(instance, "Create UI Template From Prefab");

                var parent = Selection.activeTransform;
                if (parent != null) {
                    Undo.SetTransformParent(instance.transform, parent, "Set UI Template Parent");
                    instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    instance.transform.localScale = Vector3.one;
                }

                Selection.activeObject = instance;
            }

            /// <summary>
            /// 選択中オブジェクトを命名規則でリネーム
            /// </summary>
            private void RenameSelectedObjectsByConvention() {
                var selected = Selection.gameObjects;
                for (var i = 0; i < selected.Length; i++) {
                    var obj = selected[i];
                    if (obj == null) {
                        continue;
                    }

                    var suffix = GetSuffix(obj);
                    if (string.IsNullOrEmpty(suffix) || EndsWithIgnoreCase(obj.name, suffix)) {
                        continue;
                    }

                    EditorSupportTool.RecordAndDirty(obj, "UI Auto Rename");
                    obj.name += suffix;
                }
            }

            /// <summary>
            /// オブジェクト種別に応じたサフィックスを取得
            /// </summary>
            private string GetSuffix(GameObject obj) {
                if (obj.GetComponent<Button>() != null) {
                    return Window.Data.RenameButtonSuffix;
                }

                if (obj.GetComponent<TMP_Text>() != null) {
                    return Window.Data.RenameTextSuffix;
                }

                if (obj.GetComponent<Image>() != null) {
                    return Window.Data.RenameImageSuffix;
                }

                return string.Empty;
            }

            /// <summary>
            /// 文字列が指定サフィックスで終端するかを判定
            /// </summary>
            private static bool EndsWithIgnoreCase(string value, string suffix) {
                return value?.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ?? false;
            }
        }
    }
}
