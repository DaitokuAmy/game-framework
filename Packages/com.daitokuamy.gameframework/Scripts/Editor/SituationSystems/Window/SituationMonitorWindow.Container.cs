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
                public Type[] PrevSituationTypes = Array.Empty<Type>();
                public Type[] NextSituationTypes = Array.Empty<Type>();
            }

            private readonly Dictionary<IMonitoredContainer, TransitionCapture> _transitionCaptures = new();

            /// <inheritdoc/>
            public override string Label => "Container";

            /// <inheritdoc/>
            protected override void EveryUpdateInternal(SituationMonitorWindow window) {
                var containers = SituationMonitor.Containers;

                foreach (var container in containers) {
                    CaptureContainer(container, window);
                }
            }

            /// <inheritdoc/>
            protected override void DrawGuiInternal(SituationMonitorWindow window) {
                var containers = SituationMonitor.Containers;

                foreach (var container in containers) {
                    DrawContainer(container, window);
                }
            }

            /// <summary>
            /// コンテナ情報のキャプチャ
            /// </summary>
            private void CaptureContainer(IMonitoredContainer container, SituationMonitorWindow window) {
                if (!_transitionCaptures.TryGetValue(container, out var capture)) {
                    capture = new TransitionCapture();
                    _transitionCaptures.Add(container, capture);
                }

                // 遷移情報があればキャプチャーに移す
                var transitionInfo = container.CurrentTransitionInfo;
                if (transitionInfo != null) {
                    capture.Type = transitionInfo.TransitionType;
                    capture.State = transitionInfo.State;
                    capture.Step = transitionInfo.Step;
                    capture.PrevSituationTypes = transitionInfo.PrevSituations
                        .Select(x => x.GetType())
                        .ToArray();
                    capture.NextSituationTypes = transitionInfo.NextSituations
                        .Select(x => x.GetType())
                        .ToArray();
                }
            }

            /// <summary>
            /// コンテナ情報の描画
            /// </summary>
            private void DrawContainer(IMonitoredContainer container, SituationMonitorWindow window) {
                DrawFoldoutContent(container.Label, container, target => {
                    // 遷移情報
                    DrawContent("Transition", target.CurrentTransitionInfo, transitionInfo => {
                        EditorGUILayout.LabelField("IsTransitioning",　(transitionInfo != null).ToString());

                        if (_transitionCaptures.TryGetValue(target, out var capture)) {
                            EditorGUILayout.LabelField("Type", capture.Type.ToString());
                            EditorGUILayout.LabelField("State", capture.State.ToString());
                            EditorGUILayout.LabelField("Step", capture.Step.ToString());
                            for (var i = 0; i < capture.PrevSituationTypes.Length; i++) {
                                var name = window.GetTypeName(capture.PrevSituationTypes[i]);
                                EditorGUILayout.LabelField(i == 0 ? "Prev Situations" : " ", name);
                            }

                            for (var i = 0; i < capture.NextSituationTypes.Length; i++) {
                                var name = window.GetTypeName(capture.NextSituationTypes[i]);
                                EditorGUILayout.LabelField(i == 0 ? "Next Situations" : " ", name);
                            }
                        }
                    });

                    EditorGUILayout.Space();

                    // Situation情報を表示
                    DrawContent("Situation", target, c => {
                        EditorGUILayout.LabelField("Root", window.GetSituationName(c.RootSituation));

                        var current = c.Current;
                        if (current == null) {
                            EditorGUILayout.LabelField("Current", "None");
                        }
                        else {
                            void DrawHierarchy(Situation situation, string label = " ") {
                                if (situation == null) {
                                    return;
                                }

                                EditorGUILayout.LabelField(label, window.GetSituationName(situation));
                                DrawHierarchy(situation.Parent);
                            }

                            DrawHierarchy(current, "Current");
                        }

                        for (var i = 0; i < c.PreloadSituations.Count; i++) {
                            var name = window.GetSituationName(c.PreloadSituations[i]);
                            EditorGUILayout.LabelField(i == 0 ? "Preload Situations" : " ", name);
                        }

                        for (var i = 0; i < c.RunningSituations.Count; i++) {
                            var name = window.GetSituationName(c.RunningSituations[i]);
                            EditorGUILayout.LabelField(i == 0 ? "Running Situations" : " ", name);
                        }
                    });
                }, true);
            }
        }
    }
}