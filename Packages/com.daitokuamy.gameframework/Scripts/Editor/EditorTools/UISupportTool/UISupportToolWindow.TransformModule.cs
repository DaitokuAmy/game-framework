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
        private sealed class TransformModule : EditorToolModule<UISupportToolWindow, ConfigData, UserData> {
            /// <summary>タブ表示名</summary>
            public override string DisplayName => "変形";

            /// <inheritdoc/>
            public override void OnGUI() {
                EditorGUI.BeginChangeCheck();
                Window.User.EnableArrowNudge = EditorGUILayout.ToggleLeft("矢印キーで移動", Window.User.EnableArrowNudge);
                Window.User.AutoRoundOnNudge = EditorGUILayout.ToggleLeft("移動後に1pxへ自動丸め", Window.User.AutoRoundOnNudge);
                Window.User.ArrowStep = Mathf.Max(0.1f, EditorGUILayout.FloatField("矢印移動量", Window.User.ArrowStep));
                Window.User.ShiftArrowStep = Mathf.Max(1.0f, EditorGUILayout.FloatField("Shift+矢印移動量", Window.User.ShiftArrowStep));
                if (EditorGUI.EndChangeCheck()) {
                    Window.SaveState();
                }

                if (GUILayout.Button("SceneViewへフォーカス（矢印移動用）")) {
                    UISupportTool.FocusSceneView();
                }

                EditorGUILayout.HelpBox("Hierarchy選択時は上のボタンでSceneViewへフォーカスしてから矢印キーを操作してください。", MessageType.None);

                EditorGUILayout.Space(8.0f);
                if (GUILayout.Button("選択UIを1px単位で丸める")) {
                    UISupportTool.RoundSelectedRectTransforms();
                }

                if (GUILayout.Button("選択UIのScaleを1にする")) {
                    UISupportTool.NormalizeSelectedScale();
                }

                EditorGUILayout.Space(8.0f);
                if (GUILayout.Button("現在の矩形にアンカーを合わせる")) {
                    UISupportTool.FitAnchorsToCurrentRectForSelection();
                }

                if (GUILayout.Button("親にStretchアンカーを設定")) {
                    UISupportTool.SetParentStretchAnchorsForSelection();
                }

                if (GUILayout.Button("選択UIのアンカーを[0..1]に正規化")) {
                    UISupportTool.NormalizeSelectedAnchors();
                }

                if (GUILayout.Button("選択UIのサイズを親へ転送して親Fitにする")) {
                    UISupportTool.TransferSizeToParentAndFitToParentForSelection();
                }

                if (GUILayout.Button("選択中の連続オブジェクトをGroup化")) {
                    UISupportTool.GroupConsecutiveSelection("UIGroup");
                }
            }

            /// <inheritdoc/>
            public override void OnSceneGUI(SceneView sceneView) {
                if (!Window.User.EnableArrowNudge) {
                    return;
                }

                var current = Event.current;
                if (current == null) {
                    return;
                }

                if (current.type == EventType.Layout) {
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    return;
                }

                if (!IsArrowKey(current.keyCode)) {
                    return;
                }

                if (current.type == EventType.KeyUp) {
                    current.Use();
                    return;
                }

                if (current.type != EventType.KeyDown) {
                    return;
                }

                if (!TryGetArrowDelta(current, out var delta)) {
                    current.Use();
                    return;
                }

                var step = current.shift ? Window.User.ShiftArrowStep : Window.User.ArrowStep;
                NudgeSelected(delta * step);
                current.Use();
                SceneView.RepaintAll();
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
            /// 矢印キーか判定
            /// </summary>
            private static bool IsArrowKey(KeyCode keyCode) {
                return keyCode == KeyCode.LeftArrow ||
                       keyCode == KeyCode.RightArrow ||
                       keyCode == KeyCode.UpArrow ||
                       keyCode == KeyCode.DownArrow;
            }

            /// <summary>
            /// 選択中RectTransformを移動
            /// </summary>
            private void NudgeSelected(Vector2 delta) {
                var rects = UISupportTool.GetSelectedRectTransforms();
                for (var i = 0; i < rects.Count; i++) {
                    var rect = rects[i];
                    UISupportTool.RecordAndDirty(rect, "Nudge RectTransform");
                    rect.anchoredPosition += delta;
                    if (Window.User.AutoRoundOnNudge) {
                        UISupportTool.RoundRectTransform(rect);
                    }
                }
            }
        }
    }
}
