using System;
using System.IO;
using System.Reflection;
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
            private Font _targetFont;

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
                DrawAddressablesTools();
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
                Window.Data.RenameButtonPrefix = EditorGUILayout.TextField("Button接頭辞", Window.Data.RenameButtonPrefix);
                Window.Data.RenameTextPrefix = EditorGUILayout.TextField("Text接頭辞", Window.Data.RenameTextPrefix);
                Window.Data.RenameImagePrefix = EditorGUILayout.TextField("Image接頭辞", Window.Data.RenameImagePrefix);

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
                _targetFont = (Font)EditorGUILayout.ObjectField("置換先フォント", _targetFont, typeof(Font), false);

                if (GUILayout.Button("選択UIにフォントを適用")) {
                    EditorSupportTool.ReplaceFontInSelection(_targetFont);
                }
            }

            /// <summary>
            /// テンプレート生成UIを描画
            /// </summary>
            private void DrawTemplateTools() {
                EditorGUILayout.LabelField("テンプレート", EditorStyles.boldLabel);

                if (GUILayout.Button("画面テンプレートをシーンに作成")) {
                    CreateScreenTemplate();
                }

                if (GUILayout.Button("ダイアログテンプレートをシーンに作成")) {
                    CreateDialogTemplate();
                }

                Window.Data.PresenterNamespace = EditorGUILayout.TextField("Presenter名前空間", Window.Data.PresenterNamespace);
                Window.Data.PresenterBaseClass = EditorGUILayout.TextField("Presenter基底クラス", Window.Data.PresenterBaseClass);

                if (GUILayout.Button("Presenterスクリプト雛形を作成")) {
                    CreatePresenterTemplate();
                }

                Window.SaveState();
            }

            /// <summary>
            /// Addressables支援UIを描画
            /// </summary>
            private static void DrawAddressablesTools() {
                EditorGUILayout.LabelField("Addressables登録", EditorStyles.boldLabel);
                if (GUILayout.Button("選択アセットをAddressablesへ登録(既定グループ)")) {
                    RegisterSelectedAssetsToAddressables();
                }
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

                    var prefix = GetPrefix(obj);
                    if (string.IsNullOrEmpty(prefix) || obj.name.StartsWith(prefix, StringComparison.Ordinal)) {
                        continue;
                    }

                    EditorSupportTool.RecordAndDirty(obj, "UI Auto Rename");
                    obj.name = prefix + obj.name;
                }
            }

            /// <summary>
            /// オブジェクト種別に応じたプレフィックスを取得
            /// </summary>
            private string GetPrefix(GameObject obj) {
                if (obj.GetComponent<Button>() != null) {
                    return Window.Data.RenameButtonPrefix;
                }

                if (obj.GetComponent<Text>() != null) {
                    return Window.Data.RenameTextPrefix;
                }

                if (obj.GetComponent<Image>() != null) {
                    return Window.Data.RenameImagePrefix;
                }

                return string.Empty;
            }

            /// <summary>
            /// Screenテンプレートを作成
            /// </summary>
            private static void CreateScreenTemplate() {
                var root = new GameObject("UIScreen", typeof(RectTransform), typeof(CanvasGroup));
                var panel = new GameObject("Root", typeof(RectTransform));
                panel.transform.SetParent(root.transform, false);

                var rootRect = (RectTransform)root.transform;
                var panelRect = (RectTransform)panel.transform;
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;

                Undo.RegisterCreatedObjectUndo(root, "Create UI Screen Template");
                Selection.activeObject = root;
            }

            /// <summary>
            /// Dialogテンプレートを作成
            /// </summary>
            private static void CreateDialogTemplate() {
                var root = new GameObject("UIDialog", typeof(RectTransform), typeof(CanvasGroup));
                var dimmer = new GameObject("Dimmer", typeof(RectTransform), typeof(Image));
                var content = new GameObject("Content", typeof(RectTransform), typeof(Image));

                dimmer.transform.SetParent(root.transform, false);
                content.transform.SetParent(root.transform, false);

                var dimmerRect = (RectTransform)dimmer.transform;
                dimmerRect.anchorMin = Vector2.zero;
                dimmerRect.anchorMax = Vector2.one;
                dimmerRect.offsetMin = Vector2.zero;
                dimmerRect.offsetMax = Vector2.zero;

                var contentRect = (RectTransform)content.transform;
                contentRect.anchorMin = new Vector2(0.5f, 0.5f);
                contentRect.anchorMax = new Vector2(0.5f, 0.5f);
                contentRect.sizeDelta = new Vector2(600.0f, 400.0f);

                var dimmerImage = dimmer.GetComponent<Image>();
                dimmerImage.color = new Color(0.0f, 0.0f, 0.0f, 0.6f);

                Undo.RegisterCreatedObjectUndo(root, "Create UI Dialog Template");
                Selection.activeObject = root;
            }

            /// <summary>
            /// Presenter雛形スクリプトを作成
            /// </summary>
            private void CreatePresenterTemplate() {
                var scriptName = "NewUIScreenPresenter";
                var path = EditorUtility.SaveFilePanelInProject(
                    "Create Presenter",
                    scriptName,
                    "cs",
                    "Choose save path.",
                    "Assets");

                if (string.IsNullOrEmpty(path)) {
                    return;
                }

                var className = Path.GetFileNameWithoutExtension(path);
                var script =
$"using UnityEngine;\n\nnamespace {Window.Data.PresenterNamespace} {{\n    public sealed class {className} : {Window.Data.PresenterBaseClass} {{\n    }}\n}}\n";
                File.WriteAllText(path, script);
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
            }

            /// <summary>
            /// 選択中アセットをAddressablesへ登録
            /// </summary>
            private static void RegisterSelectedAssetsToAddressables() {
                var settingsType = Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
                if (settingsType == null) {
                    Debug.LogWarning("[UISupport] Addressables package not found.");
                    return;
                }

                var settingsProperty = settingsType.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static);
                var settings = settingsProperty?.GetValue(null);
                if (settings == null) {
                    Debug.LogWarning("[UISupport] Addressables settings not found.");
                    return;
                }

                var groupProperty = settings.GetType().GetProperty("DefaultGroup", BindingFlags.Public | BindingFlags.Instance);
                var group = groupProperty?.GetValue(settings);
                if (group == null) {
                    Debug.LogWarning("[UISupport] Addressables API was not resolved.");
                    return;
                }

                var methods = settings.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                MethodInfo createOrMoveEntry = null;
                for (var i = 0; i < methods.Length; i++) {
                    if (methods[i].Name != "CreateOrMoveEntry") {
                        continue;
                    }

                    var parameters = methods[i].GetParameters();
                    if (parameters.Length == 4 && parameters[0].ParameterType == typeof(string)) {
                        createOrMoveEntry = methods[i];
                        break;
                    }
                }

                if (createOrMoveEntry == null) {
                    Debug.LogWarning("[UISupport] Addressables API was not resolved.");
                    return;
                }

                var selected = Selection.objects;
                var count = 0;

                for (var i = 0; i < selected.Length; i++) {
                    var path = AssetDatabase.GetAssetPath(selected[i]);
                    if (string.IsNullOrEmpty(path)) {
                        continue;
                    }

                    var guid = AssetDatabase.AssetPathToGUID(path);
                    if (string.IsNullOrEmpty(guid)) {
                        continue;
                    }

                    createOrMoveEntry.Invoke(settings, new[] { guid, group, false, false });
                    count++;
                }

                Debug.Log($"[UISupport] Addressables registered: {count}");
            }
        }
    }
}

