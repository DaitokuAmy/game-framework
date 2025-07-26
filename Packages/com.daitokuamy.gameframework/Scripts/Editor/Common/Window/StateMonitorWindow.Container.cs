using System.Collections.Generic;
using GameFramework.Core;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// Stateの情報監視用ウィンドウ
    /// </summary>
    partial class StateMonitorWindow {
        /// <summary>
        /// コンテナ情報描画用パネル
        /// </summary>
        private class ContainerPanel : PanelBase {
            private readonly Dictionary<IMonitoredStateContainer, IMonitoredStateContainer.TransitionInfo> _transitionInfos = new();
            private readonly List<(string label, string text)> _tempLines = new(16);
            
            private GUIStyle _richLabelStyle;

            /// <inheritdoc/>
            public override string Label => "Container";

            /// <inheritdoc/>
            protected override void StartInternal(StateMonitorWindow window, IScope scope) {
                _richLabelStyle = new GUIStyle(GUI.skin.label);
                _richLabelStyle.richText = true;
            }

            /// <inheritdoc/>
            protected override void EveryUpdateInternal(StateMonitorWindow window) {
                var containers = StateMonitor.Containers;

                foreach (var container in containers) {
                    CaptureContainer(container, window);
                }
            }

            /// <inheritdoc/>
            protected override void DrawGuiInternal(StateMonitorWindow window) {
                var containers = StateMonitor.Containers;

                foreach (var container in containers) {
                    DrawContainer(container, window);
                }
            }

            /// <summary>
            /// コンテナ情報のキャプチャ
            /// </summary>
            private void CaptureContainer(IMonitoredStateContainer container, StateMonitorWindow window) {
                if (!_transitionInfos.TryGetValue(container, out var transitionInfo)) {
                    transitionInfo = new IMonitoredStateContainer.TransitionInfo();
                    _transitionInfos.Add(container, transitionInfo);
                }

                // 遷移情報を取得
                container.GetTransitionInfo(out transitionInfo);
            }

            /// <summary>
            /// コンテナ情報の描画
            /// </summary>
            private void DrawContainer(IMonitoredStateContainer container, StateMonitorWindow window) {
                DrawFoldoutContent(container.Label, container, target => {
                    // 基本情報
                    EditorGUILayout.LabelField("Is Transitioning",　target.IsTransitioning.ToString());
                    EditorGUILayout.LabelField("Current State Info",　target.CurrentStateInfo);
                    
                    // 遷移情報
                    if (_transitionInfos.TryGetValue(target, out var transitionInfo)) {
                        DrawContent("Transition", transitionInfo, info => {
                            EditorGUILayout.LabelField("Direction", info.Direction.ToString());
                            EditorGUILayout.LabelField("State", info.State.ToString());
                            EditorGUILayout.LabelField("End Step", info.EndStep.ToString());
                            DrawMultiLines("Prev State Info", info.PrevStateInfo);
                            DrawMultiLines("Next State Info", info.NextStateInfo);
                        });
                    }

                    EditorGUILayout.Space();

                    // 詳細情報
                    _tempLines.Clear();
                    target.GetDetails(_tempLines);
                    if (_tempLines.Count > 0) {
                        DrawFoldoutContent("Details", $"{target.GetHashCode()}.Details", _tempLines, lines => {
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