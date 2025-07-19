using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.SituationSystems;
using UnityEditor;

namespace GameFramework.Editor {
    /// <summary>
    /// Situationの情報監視用ウィンドウ
    /// </summary>
    partial class SituationMonitorWindow {
        /// <summary>
        /// コンテナ情報描画用パネル
        /// </summary>
        private class ContainerPanel : PanelBase {
            /// <summary>
            /// 遷移情報のキャプチャ
            /// </summary>
            private class TransitionCapture {
                public TransitionType Type;
                public TransitionStep Step;
                public TransitionState State;
                public string[] PrevSituationNames = Array.Empty<string>();
                public string[] NextSituationNames = Array.Empty<string>();
            }

            private readonly Dictionary<IMonitoredContainer, TransitionCapture> _transitionCaptures = new();

            /// <inheritdoc/>
            public override string Label => "Container";

            /// <inheritdoc/>
            protected override void DrawGuiInternal(SituationMonitorWindow window) {
                var containers = SituationMonitor.Containers;

                foreach (var container in containers) {
                    DrawContainer(container);
                }
            }

            /// <summary>
            /// コンテナ情報の描画
            /// </summary>
            private void DrawContainer(IMonitoredContainer container) {
                DrawFoldoutContent(container.Label, container, target => {
                    // 遷移情報
                    DrawContent("Transition", target.CurrentTransitionInfo, transitionInfo => {
                        EditorGUILayout.LabelField("IsTransitioning",　(transitionInfo != null).ToString());

                        if (!_transitionCaptures.TryGetValue(target, out var capture)) {
                            capture = new TransitionCapture();
                            _transitionCaptures.Add(target, capture);
                        }

                        // 遷移情報があればキャプチャーに移す
                        if (transitionInfo != null) {
                            capture.Type = transitionInfo.TransitionType;
                            capture.State = transitionInfo.State;
                            capture.Step = transitionInfo.Step;
                            capture.PrevSituationNames = transitionInfo.PrevSituations
                                .Select(x => x.GetType().ToString())
                                .ToArray();
                            capture.NextSituationNames = transitionInfo.NextSituations
                                .Select(x => x.GetType().ToString())
                                .ToArray();
                        }

                        EditorGUILayout.LabelField("Type", capture.Type.ToString());
                        EditorGUILayout.LabelField("State", capture.State.ToString());
                        EditorGUILayout.LabelField("Step", capture.Step.ToString());
                        for (var i = 0; i < capture.PrevSituationNames.Length; i++) {
                            var name = capture.PrevSituationNames[i];
                            EditorGUILayout.LabelField(i == 0 ? "Prev Situations" : " ", name);
                        }

                        for (var i = 0; i < capture.NextSituationNames.Length; i++) {
                            var name = capture.NextSituationNames[i];
                            EditorGUILayout.LabelField(i == 0 ? "Next Situations" : " ", name);
                        }
                    });

                    EditorGUILayout.Space();

                    // Situation情報を表示
                    EditorGUILayout.LabelField("Root Situation", target.RootSituation?.GetType().ToString() ?? "None");
                    DrawContent("Current Situation", target.Current, current => {
                        if (current == null) {
                            EditorGUILayout.LabelField("None");
                        }
                        else {
                            void DrawHierarchy(Situation situation) {
                                if (situation == null) {
                                    return;
                                }

                                EditorGUI.indentLevel++;
                                EditorGUILayout.LabelField(situation.GetType().Name);
                                DrawHierarchy(situation.Parent);
                                EditorGUI.indentLevel--;
                            }

                            EditorGUI.indentLevel--;
                            DrawHierarchy(current);
                            EditorGUI.indentLevel++;
                        }
                    });
                    for (var i = 0; i < target.PreloadSituations.Count; i++) {
                        var name = target.PreloadSituations[i].GetType().Name;
                        EditorGUILayout.LabelField(i == 0 ? "Preload Situations" : " ", name);
                    }

                    for (var i = 0; i < target.RunningSituations.Count; i++) {
                        var name = target.RunningSituations[i].GetType().Name;
                        EditorGUILayout.LabelField(i == 0 ? "Running Situations" : " ", name);
                    }
                });
            }
        }
    }
}