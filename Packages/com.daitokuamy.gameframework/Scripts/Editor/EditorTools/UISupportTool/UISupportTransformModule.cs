using UnityEditor;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UISupportToolWindowのRectTransform編集モジュール
    /// </summary>
    internal sealed partial class UISupportToolWindow {
        /// <summary>
        /// RectTransform編集系の支援モジュール
        /// </summary>
        private sealed class TransformModule : EditorToolModule<UISupportToolWindow, UISupportUserData> {
            /// <summary>タブ表示名</summary>
            public override string DisplayName => "変形";

            /// <summary>
            /// GUIを描画
            /// </summary>
            public override void OnGUI() {
                Window.Data.EnableArrowNudge = EditorGUILayout.ToggleLeft("矢印キーで移動", Window.Data.EnableArrowNudge);
                Window.Data.AutoRoundOnNudge = EditorGUILayout.ToggleLeft("移動後に1pxへ自動丸め", Window.Data.AutoRoundOnNudge);
                Window.Data.ArrowStep = Mathf.Max(0.1f, EditorGUILayout.FloatField("矢印移動量", Window.Data.ArrowStep));
                Window.Data.ShiftArrowStep = Mathf.Max(1.0f, EditorGUILayout.FloatField("Shift+矢印移動量", Window.Data.ShiftArrowStep));

                EditorGUILayout.Space(8.0f);
                if (GUILayout.Button("選択UIを1px単位で丸める")) {
                    EditorSupportTool.RoundSelectedRectTransforms();
                }

                if (GUILayout.Button("選択UIのScaleを1にする")) {
                    EditorSupportTool.NormalizeSelectedScale();
                }

                EditorGUILayout.Space(8.0f);
                if (GUILayout.Button("現在の矩形にアンカーを合わせる")) {
                    EditorSupportTool.FitAnchorsToCurrentRectForSelection();
                }

                if (GUILayout.Button("親にStretchアンカーを設定")) {
                    EditorSupportTool.SetParentStretchAnchorsForSelection();
                }

                Window.Data.SafeAreaPresetIndex = EditorGUILayout.Popup(
                    "SafeAreaプリセット",
                    Window.Data.SafeAreaPresetIndex,
                    System.Array.ConvertAll(EditorSupportTool.SafeAreaPresets, x => x.Label));

                if (GUILayout.Button("選択UIへSafeAreaアンカーを適用")) {
                    EditorSupportTool.ApplySafeAreaAnchorsForSelection(Window.Data.SafeAreaPresetIndex);
                }

                if (GUILayout.Button("選択UIのアンカーを[0..1]に正規化")) {
                    EditorSupportTool.NormalizeSelectedAnchors();
                }

                Window.SaveState();
            }

            /// <summary>
            /// SceneView GUIを描画
            /// </summary>
            public override void OnSceneGUI(SceneView sceneView) {
                if (!Window.Data.EnableArrowNudge) {
                    return;
                }

                var current = Event.current;
                if (current == null || current.type != EventType.KeyDown) {
                    return;
                }

                if (!TryGetArrowDelta(current, out var delta)) {
                    return;
                }

                var step = current.shift ? Window.Data.ShiftArrowStep : Window.Data.ArrowStep;
                NudgeSelected(delta * step);
                current.Use();
            }

            /// <summary>
            /// 矢印キー入力から移動方向を取得
            /// </summary>
            private static bool TryGetArrowDelta(Event current, out Vector2 delta) {
                delta = Vector2.zero;
                switch (current.keyCode) {
                    case KeyCode.LeftArrow:
                        delta = new Vector2(-1.0f, 0.0f);
                        return true;
                    case KeyCode.RightArrow:
                        delta = new Vector2(1.0f, 0.0f);
                        return true;
                    case KeyCode.UpArrow:
                        delta = new Vector2(0.0f, 1.0f);
                        return true;
                    case KeyCode.DownArrow:
                        delta = new Vector2(0.0f, -1.0f);
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// 選択中RectTransformを移動
            /// </summary>
            private void NudgeSelected(Vector2 delta) {
                var rects = EditorSupportTool.GetSelectedRectTransforms();
                for (var i = 0; i < rects.Count; i++) {
                    var rect = rects[i];
                    EditorSupportTool.RecordAndDirty(rect, "Nudge RectTransform");
                    rect.anchoredPosition += delta;
                    if (Window.Data.AutoRoundOnNudge) {
                        EditorSupportTool.RoundRectTransform(rect);
                    }
                }
            }
        }
    }
}

