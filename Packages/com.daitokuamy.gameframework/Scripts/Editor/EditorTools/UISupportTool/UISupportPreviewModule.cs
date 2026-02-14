using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UISupportToolWindowの可視化モジュール
    /// </summary>
    internal sealed partial class UISupportToolWindow {
        /// <summary>
        /// 可視化と簡易プレビューを行うモジュール
        /// </summary>
        private sealed class PreviewModule : EditorToolModule<UISupportToolWindow, UISupportUserData> {
            private readonly Dictionary<RectTransform, Vector3> _originalScales = new();
            private readonly Dictionary<CanvasGroup, float> _originalAlphas = new();

            private bool _previewPlaying;
            private double _previewStartTime;

            /// <summary>タブ表示名</summary>
            public override string DisplayName => "プレビュー";

            /// <summary>
            /// GUIを描画
            /// </summary>
            public override void OnGUI() {
                Window.Data.DrawAnchorVisualization = EditorGUILayout.ToggleLeft("SceneViewでAnchor/Padding可視化", Window.Data.DrawAnchorVisualization);
                Window.Data.DrawSafeAreaSimulation = EditorGUILayout.ToggleLeft("SceneViewでSafeAreaシミュレーション表示", Window.Data.DrawSafeAreaSimulation);
                Window.Data.SafeAreaPresetIndex = EditorGUILayout.Popup(
                    "SafeAreaプリセット",
                    Window.Data.SafeAreaPresetIndex,
                    System.Array.ConvertAll(EditorSupportTool.SafeAreaPresets, x => x.Label));

                EditorGUILayout.Space(8.0f);
                Window.Data.PreviewDuration = Mathf.Max(0.05f, EditorGUILayout.FloatField("再生時間", Window.Data.PreviewDuration));
                Window.Data.PreviewScale = Mathf.Max(0.1f, EditorGUILayout.FloatField("スケール", Window.Data.PreviewScale));
                Window.Data.PreviewUseFade = EditorGUILayout.ToggleLeft("フェードを使う", Window.Data.PreviewUseFade);
                Window.Data.PreviewUseScale = EditorGUILayout.ToggleLeft("スケールを使う", Window.Data.PreviewUseScale);

                using (new EditorGUILayout.HorizontalScope()) {
                    if (GUILayout.Button("プレビュー開始")) {
                        StartPreview();
                    }

                    if (GUILayout.Button("プレビュー停止")) {
                        StopPreview();
                    }
                }

                Window.SaveState();
            }

            /// <summary>
            /// 更新処理を実行
            /// </summary>
            public override void OnUpdate() {
                if (!_previewPlaying) {
                    return;
                }

                UpdatePreview();
            }

            /// <summary>
            /// SceneView GUIを描画
            /// </summary>
            public override void OnSceneGUI(SceneView sceneView) {
                if (Window.Data.DrawAnchorVisualization) {
                    DrawAnchorVisualization();
                }

                if (Window.Data.DrawSafeAreaSimulation) {
                    DrawSafeAreaSimulation();
                }
            }

            /// <summary>
            /// モジュール終了時処理
            /// </summary>
            protected override void OnExitInternal() {
                StopPreview();
            }

            /// <summary>
            /// プレビューを開始
            /// </summary>
            private void StartPreview() {
                StopPreview();
                _previewPlaying = true;
                _previewStartTime = EditorApplication.timeSinceStartup;

                var rects = EditorSupportTool.GetSelectedRectTransforms();
                for (var i = 0; i < rects.Count; i++) {
                    if (rects[i] != null && !_originalScales.ContainsKey(rects[i])) {
                        _originalScales.Add(rects[i], rects[i].localScale);
                    }
                }

                var selected = Selection.gameObjects;
                for (var i = 0; i < selected.Length; i++) {
                    var groups = selected[i].GetComponentsInChildren<CanvasGroup>(true);
                    for (var j = 0; j < groups.Length; j++) {
                        if (groups[j] != null && !_originalAlphas.ContainsKey(groups[j])) {
                            _originalAlphas.Add(groups[j], groups[j].alpha);
                        }
                    }
                }
            }

            /// <summary>
            /// プレビューを停止
            /// </summary>
            private void StopPreview() {
                if (!_previewPlaying && _originalScales.Count == 0 && _originalAlphas.Count == 0) {
                    return;
                }

                foreach (var kv in _originalScales) {
                    if (kv.Key != null) {
                        kv.Key.localScale = kv.Value;
                    }
                }

                foreach (var kv in _originalAlphas) {
                    if (kv.Key != null) {
                        kv.Key.alpha = kv.Value;
                    }
                }

                _originalScales.Clear();
                _originalAlphas.Clear();
                _previewPlaying = false;
            }

            /// <summary>
            /// プレビュー値を更新
            /// </summary>
            private void UpdatePreview() {
                var duration = Mathf.Max(0.05f, Window.Data.PreviewDuration);
                var time = (float)(EditorApplication.timeSinceStartup - _previewStartTime);
                var normalized = Mathf.PingPong(time / duration, 1.0f);
                var eased = Mathf.SmoothStep(0.0f, 1.0f, normalized);

                if (Window.Data.PreviewUseScale) {
                    var targetScale = Window.Data.PreviewScale;
                    foreach (var kv in _originalScales) {
                        if (kv.Key == null) {
                            continue;
                        }

                        kv.Key.localScale = Vector3.LerpUnclamped(kv.Value, kv.Value * targetScale, eased);
                    }
                }

                if (Window.Data.PreviewUseFade) {
                    foreach (var kv in _originalAlphas) {
                        if (kv.Key == null) {
                            continue;
                        }

                        kv.Key.alpha = Mathf.Lerp(kv.Value, kv.Value * 0.4f, eased);
                    }
                }
            }

            /// <summary>
            /// アンカー可視化を描画
            /// </summary>
            private static void DrawAnchorVisualization() {
                var rects = EditorSupportTool.GetSelectedRectTransforms();
                Handles.color = new Color(0.2f, 0.8f, 1.0f, 0.9f);

                for (var i = 0; i < rects.Count; i++) {
                    var rect = rects[i];
                    if (rect == null || rect.parent is not RectTransform parent) {
                        continue;
                    }

                    var corners = new Vector3[4];
                    rect.GetWorldCorners(corners);
                    Handles.DrawAAPolyLine(corners[0], corners[1], corners[2], corners[3], corners[0]);

                    var parentCorners = new Vector3[4];
                    parent.GetWorldCorners(parentCorners);

                    var anchorMin = Vector3.Lerp(parentCorners[0], parentCorners[2], rect.anchorMin.x);
                    anchorMin = Vector3.Lerp(anchorMin, Vector3.Lerp(parentCorners[0], parentCorners[1], rect.anchorMin.y), 0.5f);

                    var anchorMax = Vector3.Lerp(parentCorners[0], parentCorners[2], rect.anchorMax.x);
                    anchorMax = Vector3.Lerp(anchorMax, Vector3.Lerp(parentCorners[0], parentCorners[1], rect.anchorMax.y), 0.5f);

                    Handles.DrawLine(anchorMin, anchorMax);
                }
            }

            /// <summary>
            /// SafeArea可視化を描画
            /// </summary>
            private void DrawSafeAreaSimulation() {
                var selected = EditorSupportTool.GetSelectedRectTransforms();
                for (var i = 0; i < selected.Count; i++) {
                    var rect = selected[i];
                    if (rect == null) {
                        continue;
                    }

                    var safeRect = EditorSupportTool.GetSafeAreaRect(Window.Data.SafeAreaPresetIndex, rect);
                    DrawSafeAreaRect(rect, safeRect);
                }
            }

            /// <summary>
            /// SafeArea矩形を線描画
            /// </summary>
            private static void DrawSafeAreaRect(RectTransform rectTransform, Rect localRect) {
                var transform = rectTransform.transform;

                var p0 = transform.TransformPoint(new Vector3(localRect.xMin, localRect.yMin, 0.0f));
                var p1 = transform.TransformPoint(new Vector3(localRect.xMax, localRect.yMin, 0.0f));
                var p2 = transform.TransformPoint(new Vector3(localRect.xMax, localRect.yMax, 0.0f));
                var p3 = transform.TransformPoint(new Vector3(localRect.xMin, localRect.yMax, 0.0f));

                Handles.color = new Color(1.0f, 0.7f, 0.0f, 0.95f);
                Handles.DrawAAPolyLine(p0, p1, p2, p3, p0);
            }
        }
    }
}

