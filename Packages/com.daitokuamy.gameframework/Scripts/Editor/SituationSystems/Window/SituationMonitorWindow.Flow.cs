using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// Situationの情報監視用ウィンドウ
    /// </summary>
    partial class SituationMonitorWindow {
        /// <summary>
        /// フロー情報描画用パネル
        /// </summary>
        private class FlowPanel : PanelBase {
            private readonly List<(string label, string text)> _tempLines = new(16);

            private GUIStyle _richLabelStyle;
            
            /// <inheritdoc/>
            public override string Label => "Flow";

            /// <inheritdoc/>
            protected override void StartInternal(SituationMonitorWindow window, IScope scope) {
                _richLabelStyle = new GUIStyle(GUI.skin.label);
                _richLabelStyle.richText = true;
            }

            /// <inheritdoc/>
            protected override void DrawGuiInternal(SituationMonitorWindow window) {
                var flows = SituationMonitor.Flows;

                foreach (var flow in flows) {
                    DrawFlow(flow, window);
                }
            }

            /// <summary>
            /// フロー情報の描画
            /// </summary>
            private void DrawFlow(IMonitoredFlow flow, SituationMonitorWindow window) {
                DrawFoldoutContent(flow.Label, flow, target => {
                    EditorGUILayout.LabelField("Type", flow.GetType().Name);
                    EditorGUILayout.LabelField("Back Target", window.GetSituationName(target.BackTarget));
                    _tempLines.Clear();
                    target.GetDetails(_tempLines);
                    if (_tempLines.Count > 0) {
                        DrawFoldoutContent("Details", $"{flow.GetHashCode()}.Details", _tempLines, lines => {
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