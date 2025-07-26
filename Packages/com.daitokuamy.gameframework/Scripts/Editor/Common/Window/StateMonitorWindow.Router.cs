using System.Collections.Generic;
using GameFramework.Core;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// StateMonitorWindowの情報監視用ウィンドウ
    /// </summary>
    partial class StateMonitorWindow {
        /// <summary>
        /// ルーター情報描画用パネル
        /// </summary>
        private class RouterPanel : PanelBase {
            private readonly List<(string label, string text)> _tempLines = new(16);

            private GUIStyle _richLabelStyle;

            /// <inheritdoc/>
            public override string Label => "Router";

            /// <inheritdoc/>
            protected override void StartInternal(StateMonitorWindow window, IScope scope) {
                _richLabelStyle = new GUIStyle(GUI.skin.label);
                _richLabelStyle.richText = true;
            }

            /// <inheritdoc/>
            protected override void DrawGuiInternal(StateMonitorWindow window) {
                var routeres = StateMonitor.Routers;

                foreach (var router in routeres) {
                    DrawRouter(router, window);
                }
            }

            /// <summary>
            /// フロー情報の描画
            /// </summary>
            private void DrawRouter(IMonitoredStateRouter router, StateMonitorWindow window) {
                DrawFoldoutContent(router.Label, router, target => {
                    EditorGUILayout.LabelField("Type", router.GetType().Name);
                    EditorGUILayout.LabelField("Back State Info", target.BackStateInfo);
                    _tempLines.Clear();
                    target.GetDetails(_tempLines);
                    if (_tempLines.Count > 0) {
                        DrawFoldoutContent("Details", $"{router.GetHashCode()}.Details", _tempLines, lines => {
                            for (var i = 0; i < lines.Count; i++) {
                                var label = lines[i].label;
                                var text = lines[i].text;
                                EditorGUILayout.LabelField(string.IsNullOrEmpty(label) ? " " : label, text, _richLabelStyle);
                            }
                        }, true);
                    }
                }, true);
            }
        }
    }
}