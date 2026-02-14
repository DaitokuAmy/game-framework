using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UISupportToolWindowのバインド診断モジュール
    /// </summary>
    internal sealed partial class UISupportToolWindow {
        /// <summary>
        /// SerializeFieldの自動バインド支援モジュール
        /// </summary>
        private sealed class BindingModule : EditorToolModule<UISupportToolWindow, ConfigData, UserData> {
            private readonly List<BindingIssue> _issues = new();

            /// <summary>タブ表示名</summary>
            public override string DisplayName => "バインド";

            /// <summary>
            /// 未割当フィールド情報
            /// </summary>
            private sealed class BindingIssue {
                public Component Component;
                public string FieldName;
                public Type FieldType;

                /// <summary>表示用メッセージ</summary>
                public string Message => $"{Component.GetType().Name}.{FieldName} ({FieldType.Name}) is null.";
            }

            /// <summary>
            /// GUIを描画
            /// </summary>
            public override void OnGUI() {
                EditorGUILayout.HelpBox("選択中オブジェクト配下のMonoBehaviourを対象にします。", MessageType.Info);

                if (GUILayout.Button("未設定SerializeFieldを名前一致で自動バインド")) {
                    var count = AutoBindSelected();
                    Debug.Log($"[UISupport] AutoBind complete. bound:{count}");
                    RefreshUnassignedIssues();
                }

                if (GUILayout.Button("未設定SerializeFieldを検出")) {
                    RefreshUnassignedIssues();
                }

                EditorGUILayout.Space(6.0f);
                EditorGUILayout.LabelField($"未設定フィールド数: {_issues.Count}", EditorStyles.boldLabel);

                for (var i = 0; i < _issues.Count; i++) {
                    using (new EditorGUILayout.HorizontalScope()) {
                        EditorGUILayout.LabelField(_issues[i].Message);
                        if (GUILayout.Button("選択", GUILayout.Width(48.0f))) {
                            EditorGUIUtility.PingObject(_issues[i].Component);
                            Selection.activeObject = _issues[i].Component;
                        }
                    }
                }
            }

            /// <summary>
            /// 選択中ルート配下の自動バインドを実行
            /// </summary>
            private static int AutoBindSelected() {
                var selectedRoots = Selection.gameObjects;
                var bindCount = 0;

                for (var i = 0; i < selectedRoots.Length; i++) {
                    var behaviours = selectedRoots[i].GetComponentsInChildren<MonoBehaviour>(true);
                    for (var j = 0; j < behaviours.Length; j++) {
                        var behaviour = behaviours[j];
                        if (behaviour == null) {
                            continue;
                        }

                        bindCount += AutoBindBehaviour(behaviour);
                    }
                }

                return bindCount;
            }

            /// <summary>
            /// 単一MonoBehaviourの自動バインドを実行
            /// </summary>
            private static int AutoBindBehaviour(MonoBehaviour behaviour) {
                var type = behaviour.GetType();
                var count = 0;
                foreach (var field in UISupportTool.GetSerializableObjectFields(type)) {
                    var currentValue = field.GetValue(behaviour) as UnityEngine.Object;
                    if (currentValue != null) {
                        continue;
                    }

                    var found = FindObjectByFieldName(behaviour.transform, field);
                    if (found == null) {
                        continue;
                    }

                    UISupportTool.RecordAndDirty(behaviour, "Auto Bind SerializeField");
                    field.SetValue(behaviour, found);
                    count++;
                }

                return count;
            }

            /// <summary>
            /// フィールド名に一致するObjectを探索
            /// </summary>
            private static UnityEngine.Object FindObjectByFieldName(Transform root, FieldInfo fieldInfo) {
                if (root == null || fieldInfo == null) {
                    return null;
                }

                var candidates = BuildCandidateNames(fieldInfo.Name);
                var fieldType = fieldInfo.FieldType;

                for (var i = 0; i < candidates.Count; i++) {
                    var target = FindChildByName(root, candidates[i]);
                    if (target == null) {
                        continue;
                    }

                    if (fieldType == typeof(GameObject) || fieldType.IsAssignableFrom(typeof(GameObject))) {
                        return target.gameObject;
                    }

                    if (typeof(Component).IsAssignableFrom(fieldType)) {
                        return target.GetComponent(fieldType);
                    }
                }

                return null;
            }

            /// <summary>
            /// フィールド名候補一覧を生成
            /// </summary>
            private static List<string> BuildCandidateNames(string fieldName) {
                var trimmed = fieldName.TrimStart('_');
                var removePrefix = trimmed.StartsWith("m_", StringComparison.Ordinal) ? trimmed[2..] : trimmed;
                var pascal = removePrefix.Length > 0 ? char.ToUpper(removePrefix[0]) + removePrefix[1..] : removePrefix;

                return new List<string> {
                    fieldName,
                    trimmed,
                    removePrefix,
                    pascal
                };
            }

            /// <summary>
            /// 名前一致の子Transformを探索
            /// </summary>
            private static Transform FindChildByName(Transform root, string name) {
                if (string.IsNullOrEmpty(name)) {
                    return null;
                }

                var stack = new Stack<Transform>();
                stack.Push(root);

                while (stack.Count > 0) {
                    var current = stack.Pop();
                    if (string.Equals(current.name, name, StringComparison.Ordinal) ||
                        string.Equals(current.name, name, StringComparison.OrdinalIgnoreCase)) {
                        return current;
                    }

                    for (var i = 0; i < current.childCount; i++) {
                        stack.Push(current.GetChild(i));
                    }
                }

                return null;
            }

            /// <summary>
            /// 未割当SerializeField一覧を更新
            /// </summary>
            private void RefreshUnassignedIssues() {
                _issues.Clear();

                var selectedRoots = Selection.gameObjects;
                for (var i = 0; i < selectedRoots.Length; i++) {
                    var behaviours = selectedRoots[i].GetComponentsInChildren<MonoBehaviour>(true);
                    for (var j = 0; j < behaviours.Length; j++) {
                        var behaviour = behaviours[j];
                        if (behaviour == null) {
                            continue;
                        }

                        var type = behaviour.GetType();
                        foreach (var field in UISupportTool.GetSerializableObjectFields(type)) {
                            var currentValue = field.GetValue(behaviour) as UnityEngine.Object;
                            if (currentValue == null) {
                                _issues.Add(new BindingIssue {
                                    Component = behaviour,
                                    FieldName = field.Name,
                                    FieldType = field.FieldType
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}
