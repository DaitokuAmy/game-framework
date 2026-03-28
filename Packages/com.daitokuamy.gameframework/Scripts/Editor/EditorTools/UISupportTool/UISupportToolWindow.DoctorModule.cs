using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UISupportToolWindowの診断モジュール
    /// </summary>
    internal sealed partial class UISupportToolWindow {
        /// <summary>
        /// UI構成の診断を行うモジュール
        /// </summary>
        private sealed class DoctorModule : EditorToolModule<UISupportToolWindow, ConfigData, UserData> {
            private readonly List<DoctorIssue> _issues = new();

            /// <summary>タブ表示名</summary>
            public override string DisplayName => "診断";

            /// <summary>
            /// 診断結果情報
            /// </summary>
            private sealed class DoctorIssue {
                public MessageType MessageType;
                public string Message;
                public Object Context;
            }

            /// <inheritdoc/>
            public override void OnGUI() {
                EditorGUILayout.HelpBox("UI Doctor: 選択中オブジェクト配下をまとめて診断します。", MessageType.Info);

                if (GUILayout.Button("UIドクターを実行")) {
                    RunDoctor();
                }

                if (GUILayout.Button("選択ボタンのNavigationをNoneにする")) {
                    UISupportTool.SetButtonNavigationNoneForSelection();
                }

                EditorGUILayout.Space(6.0f);
                EditorGUILayout.LabelField($"問題数: {_issues.Count}", EditorStyles.boldLabel);

                for (var i = 0; i < _issues.Count; i++) {
                    using (new EditorGUILayout.HorizontalScope()) {
                        EditorGUILayout.HelpBox(_issues[i].Message, _issues[i].MessageType);
                        if (_issues[i].Context != null && GUILayout.Button("選択", GUILayout.Width(48.0f))) {
                            EditorGUIUtility.PingObject(_issues[i].Context);
                            Selection.activeObject = _issues[i].Context;
                        }
                    }
                }
            }

            /// <summary>
            /// 診断を実行
            /// </summary>
            private void RunDoctor() {
                _issues.Clear();

                var roots = Selection.gameObjects;
                for (var i = 0; i < roots.Length; i++) {
                    RunDoctorForRoot(roots[i]);
                }

                CheckCanvasDuplication();
            }

            /// <summary>
            /// 指定ルート配下の診断を実行
            /// </summary>
            private void RunDoctorForRoot(GameObject root) {
                if (root == null) {
                    return;
                }

                var rects = root.GetComponentsInChildren<RectTransform>(true);
                for (var i = 0; i < rects.Length; i++) {
                    var rect = rects[i];
                    if (rect == null) {
                        continue;
                    }

                    if (rect.localScale != Vector3.one) {
                        AddIssue(MessageType.Warning, "RectTransform scale is not 1.0.", rect);
                    }

                    var layoutGroup = rect.GetComponent<LayoutGroup>();
                    var contentSizeFitter = rect.GetComponent<ContentSizeFitter>();
                    if (layoutGroup != null && contentSizeFitter != null) {
                        AddIssue(MessageType.Warning, "LayoutGroup and ContentSizeFitter are both attached.", rect);
                    }
                }

                var raycasters = root.GetComponentsInChildren<GraphicRaycaster>(true);
                for (var i = 0; i < raycasters.Length; i++) {
                    var raycaster = raycasters[i];
                    if (raycaster == null) {
                        continue;
                    }

                    var graphics = raycaster.GetComponentsInChildren<Graphic>(true);
                    var hasRaycastTarget = false;
                    for (var j = 0; j < graphics.Length; j++) {
                        if (graphics[j] != null && graphics[j].raycastTarget) {
                            hasRaycastTarget = true;
                            break;
                        }
                    }

                    if (!hasRaycastTarget) {
                        AddIssue(MessageType.Info, "GraphicRaycaster may be unnecessary (no raycastTarget graphic).", raycaster);
                    }
                }

                var buttons = root.GetComponentsInChildren<Button>(true);
                for (var i = 0; i < buttons.Length; i++) {
                    if (buttons[i] != null && buttons[i].navigation.mode != Navigation.Mode.None) {
                        AddIssue(MessageType.Info, "Button navigation is not None.", buttons[i]);
                    }
                }

                var images = root.GetComponentsInChildren<Image>(true);
                for (var i = 0; i < images.Length; i++) {
                    var image = images[i];
                    if (image == null || image.sprite == null) {
                        continue;
                    }

                    if (!image.sprite.packed) {
                        AddIssue(MessageType.Info, "Sprite is not packed to atlas.", image);
                    }
                }

                CheckMissingScripts(root);
            }

            /// <summary>
            /// Missing Scriptを検出
            /// </summary>
            private void CheckMissingScripts(GameObject root) {
                var transforms = root.GetComponentsInChildren<Transform>(true);
                for (var i = 0; i < transforms.Length; i++) {
                    var components = transforms[i].GetComponents<Component>();
                    for (var j = 0; j < components.Length; j++) {
                        if (components[j] == null) {
                            AddIssue(MessageType.Error, "Missing Script detected.", transforms[i]);
                            break;
                        }
                    }
                }
            }

            /// <summary>
            /// Canvas重複とSortingOrder異常を検出
            /// </summary>
            private void CheckCanvasDuplication() {
                var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var rootCanvases = new List<Canvas>();
                var sortingOrderMap = new Dictionary<int, int>();

                for (var i = 0; i < canvases.Length; i++) {
                    if (canvases[i] != null && canvases[i].isRootCanvas) {
                        rootCanvases.Add(canvases[i]);
                    }
                }

                if (rootCanvases.Count > 1) {
                    AddIssue(MessageType.Warning, $"Multiple root canvases found: {rootCanvases.Count}", rootCanvases[0]);
                }

                for (var i = 0; i < rootCanvases.Count; i++) {
                    var order = rootCanvases[i].sortingOrder;
                    sortingOrderMap.TryGetValue(order, out var count);
                    sortingOrderMap[order] = count + 1;

                    if (Mathf.Abs(order) > 1000) {
                        AddIssue(MessageType.Warning, $"SortingOrder is large: {order}", rootCanvases[i]);
                    }
                }

                foreach (var kv in sortingOrderMap) {
                    if (kv.Value > 1) {
                        AddIssue(MessageType.Info, $"Duplicate sortingOrder detected: {kv.Key} ({kv.Value} canvases)", null);
                    }
                }
            }

            /// <summary>
            /// 診断結果を追加
            /// </summary>
            private void AddIssue(MessageType messageType, string message, Object context) {
                _issues.Add(new DoctorIssue {
                    MessageType = messageType,
                    Message = message,
                    Context = context
                });
            }
        }
    }
}
