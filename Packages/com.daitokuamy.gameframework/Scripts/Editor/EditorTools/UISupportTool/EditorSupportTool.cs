using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UI編集を補助する再利用可能なEditor機能群
    /// </summary>
    public static class EditorSupportTool {
        /// <summary>
        /// SceneViewへフォーカスを移動
        /// </summary>
        public static void FocusSceneView() {
            EditorApplication.delayCall += FocusSceneViewInternal;
        }

        /// <summary>
        /// SceneViewへのフォーカスを遅延実行
        /// </summary>
        private static void FocusSceneViewInternal() {
            FocusSceneViewWindow();
            EditorApplication.delayCall += FocusSceneViewWindow;
        }

        /// <summary>
        /// SceneViewウィンドウへフォーカスを設定
        /// </summary>
        private static void FocusSceneViewWindow() {
            SceneView.FocusWindowIfItsOpen<SceneView>();

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) {
                sceneView = EditorWindow.GetWindow<SceneView>();
            }

            if (sceneView == null) {
                return;
            }

            GUI.FocusControl(null);
            sceneView.wantsMouseMove = true;
            sceneView.Focus();
            sceneView.Repaint();
            SceneView.RepaintAll();
            sceneView.SendEvent(new Event {
                type = EventType.MouseMove,
                mousePosition = sceneView.position.size * 0.5f
            });
            SimulateSceneViewClickKeepingSelection(sceneView);
        }

        /// <summary>
        /// 選択状態を維持したままSceneViewへ疑似クリックを送信
        /// </summary>
        private static void SimulateSceneViewClickKeepingSelection(SceneView sceneView) {
            if (sceneView == null) {
                return;
            }

            var previousSelection = Selection.objects;
            var mousePosition = sceneView.position.size * 0.5f;

            sceneView.SendEvent(new Event {
                type = EventType.MouseDown,
                button = 0,
                mousePosition = mousePosition
            });
            sceneView.SendEvent(new Event {
                type = EventType.MouseUp,
                button = 0,
                mousePosition = mousePosition
            });

            EditorApplication.delayCall += () => {
                if (previousSelection != null) {
                    Selection.objects = previousSelection;
                }
            };
        }

        /// <summary>
        /// 選択中RectTransform一覧を取得
        /// </summary>
        public static IReadOnlyList<RectTransform> GetSelectedRectTransforms() {
            var selectedGameObjects = Selection.gameObjects;
            var list = new List<RectTransform>(selectedGameObjects.Length);
            for (var i = 0; i < selectedGameObjects.Length; i++) {
                if (selectedGameObjects[i] == null) {
                    continue;
                }

                var rectTransform = selectedGameObjects[i].GetComponent<RectTransform>();
                if (rectTransform != null) {
                    list.Add(rectTransform);
                }
            }

            return list;
        }

        /// <summary>
        /// 選択中RectTransform一覧を深く取得
        /// </summary>
        public static RectTransform[] GetSelectedRectTransformsDeep() {
            return Selection.GetFiltered<RectTransform>(SelectionMode.Editable | SelectionMode.Deep);
        }

        /// <summary>
        /// フィールドがSerialize対象Object参照か判定
        /// </summary>
        public static bool IsSerializableObjectField(FieldInfo fieldInfo) {
            if (fieldInfo == null) {
                return false;
            }

            if (!typeof(Object).IsAssignableFrom(fieldInfo.FieldType)) {
                return false;
            }

            if (fieldInfo.IsStatic || fieldInfo.IsNotSerialized) {
                return false;
            }

            if (fieldInfo.IsPublic) {
                return true;
            }

            return fieldInfo.GetCustomAttribute<SerializeField>() != null;
        }

        /// <summary>
        /// Serialize対象Object参照フィールド列挙
        /// </summary>
        public static IEnumerable<FieldInfo> GetSerializableObjectFields(Type type) {
            if (type == null) {
                yield break;
            }

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fields = type.GetFields(flags);
            for (var i = 0; i < fields.Length; i++) {
                if (IsSerializableObjectField(fields[i])) {
                    yield return fields[i];
                }
            }
        }

        /// <summary>
        /// Undo記録とdirty化を実行
        /// </summary>
        public static void RecordAndDirty(Object target, string undoLabel) {
            if (target == null) {
                return;
            }

            Undo.RecordObject(target, undoLabel);
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// 選択中UIのLayout再計算を実行
        /// </summary>
        public static void RebuildLayoutsInSelection() {
            var rects = GetSelectedRectTransformsDeep();
            for (var i = 0; i < rects.Length; i++) {
                if (rects[i] != null) {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rects[i]);
                }
            }
        }

        /// <summary>
        /// RectTransform値をピクセル単位で丸める
        /// </summary>
        public static void RoundRectTransform(RectTransform rect) {
            if (rect == null) {
                return;
            }

            RecordAndDirty(rect, "Round RectTransform Values");
            rect.anchoredPosition = Round(rect.anchoredPosition);
            rect.sizeDelta = Round(rect.sizeDelta);
            rect.offsetMin = Round(rect.offsetMin);
            rect.offsetMax = Round(rect.offsetMax);
            rect.localPosition = Round(rect.localPosition);
        }

        /// <summary>
        /// 選択中RectTransform値をピクセル単位で丸める
        /// </summary>
        public static void RoundSelectedRectTransforms() {
            var rects = GetSelectedRectTransforms();
            for (var i = 0; i < rects.Count; i++) {
                RoundRectTransform(rects[i]);
            }
        }

        /// <summary>
        /// RectTransformのScaleを1に設定
        /// </summary>
        public static void NormalizeScale(RectTransform rect) {
            if (rect == null) {
                return;
            }

            RecordAndDirty(rect, "Normalize RectTransform Scale");
            rect.localScale = Vector3.one;
        }

        /// <summary>
        /// 選択中RectTransformのScaleを1に設定
        /// </summary>
        public static void NormalizeSelectedScale() {
            var rects = GetSelectedRectTransforms();
            for (var i = 0; i < rects.Count; i++) {
                NormalizeScale(rects[i]);
            }
        }

        /// <summary>
        /// 現在矩形へアンカーをフィット
        /// </summary>
        public static void FitAnchorsToCurrentRect(RectTransform rect) {
            if (rect == null || rect.parent is not RectTransform parent) {
                return;
            }

            var parentSize = parent.rect.size;
            if (Mathf.Abs(parentSize.x) < 0.0001f || Mathf.Abs(parentSize.y) < 0.0001f) {
                return;
            }

            var anchorMin = rect.anchorMin + new Vector2(rect.offsetMin.x / parentSize.x, rect.offsetMin.y / parentSize.y);
            var anchorMax = rect.anchorMax + new Vector2(rect.offsetMax.x / parentSize.x, rect.offsetMax.y / parentSize.y);

            RecordAndDirty(rect, "Fit Anchors To Current Rect");
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 選択中RectTransformのアンカーを現在矩形へフィット
        /// </summary>
        public static void FitAnchorsToCurrentRectForSelection() {
            var rects = GetSelectedRectTransforms();
            for (var i = 0; i < rects.Count; i++) {
                FitAnchorsToCurrentRect(rects[i]);
            }
        }

        /// <summary>
        /// 親いっぱいのStretchアンカーを設定
        /// </summary>
        public static void SetParentStretchAnchors(RectTransform rect) {
            if (rect == null || rect.parent is not RectTransform) {
                return;
            }

            RecordAndDirty(rect, "Set Parent Stretch Anchors");
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 選択中RectTransformに親Stretchアンカーを設定
        /// </summary>
        public static void SetParentStretchAnchorsForSelection() {
            var rects = GetSelectedRectTransforms();
            for (var i = 0; i < rects.Count; i++) {
                SetParentStretchAnchors(rects[i]);
            }
        }

        /// <summary>
        /// RectTransformアンカーを[0..1]へ正規化
        /// </summary>
        public static void NormalizeAnchors(RectTransform rect) {
            if (rect == null) {
                return;
            }

            RecordAndDirty(rect, "Normalize Anchors");
            rect.anchorMin = new Vector2(Mathf.Clamp01(rect.anchorMin.x), Mathf.Clamp01(rect.anchorMin.y));
            rect.anchorMax = new Vector2(Mathf.Clamp01(rect.anchorMax.x), Mathf.Clamp01(rect.anchorMax.y));
        }

        /// <summary>
        /// 選択中RectTransformアンカーを[0..1]へ正規化
        /// </summary>
        public static void NormalizeSelectedAnchors() {
            var rects = GetSelectedRectTransforms();
            for (var i = 0; i < rects.Count; i++) {
                NormalizeAnchors(rects[i]);
            }
        }

        /// <summary>
        /// ButtonのNavigationをNone化
        /// </summary>
        public static void SetButtonNavigationNone(Button button) {
            if (button == null) {
                return;
            }

            RecordAndDirty(button, "Set Button Navigation None");
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
        }

        /// <summary>
        /// 選択中ButtonのNavigationをNone化
        /// </summary>
        public static void SetButtonNavigationNoneForSelection() {
            var selected = Selection.gameObjects;
            for (var i = 0; i < selected.Length; i++) {
                var buttons = selected[i].GetComponentsInChildren<Button>(true);
                for (var j = 0; j < buttons.Length; j++) {
                    SetButtonNavigationNone(buttons[j]);
                }
            }
        }

        /// <summary>
        /// 選択中TMP_Textのフォントを置換
        /// </summary>
        public static void ReplaceFontInSelection(TMP_FontAsset font) {
            ReplaceFontInSelection(font, null, false);
        }

        /// <summary>
        /// 選択中TMP_Textのフォントとマテリアルを置換
        /// </summary>
        public static void ReplaceFontInSelection(TMP_FontAsset font, Material material, bool applyMaterial) {
            if (font == null && (!applyMaterial || material == null)) {
                return;
            }

            var selected = Selection.gameObjects;
            for (var i = 0; i < selected.Length; i++) {
                var texts = selected[i].GetComponentsInChildren<TMP_Text>(true);
                for (var j = 0; j < texts.Length; j++) {
                    if (texts[j] == null) {
                        continue;
                    }

                    var willChangeFont = font != null && texts[j].font != font;
                    var willChangeMaterial = applyMaterial && material != null && texts[j].fontSharedMaterial != material;
                    if (!willChangeFont && !willChangeMaterial) {
                        continue;
                    }

                    RecordAndDirty(texts[j], "Replace TMP Font/Material");

                    if (willChangeFont) {
                        texts[j].font = font;
                    }

                    if (willChangeMaterial) {
                        texts[j].fontSharedMaterial = material;
                    }
                }
            }
        }

        /// <summary>
        /// Vector2を丸める
        /// </summary>
        public static Vector2 Round(Vector2 value) {
            return new Vector2(Mathf.Round(value.x), Mathf.Round(value.y));
        }

        /// <summary>
        /// Vector3を丸める
        /// </summary>
        public static Vector3 Round(Vector3 value) {
            return new Vector3(Mathf.Round(value.x), Mathf.Round(value.y), Mathf.Round(value.z));
        }

        /// <summary>
        /// RectTransformコンテキストでピクセル丸めを実行
        /// </summary>
        [MenuItem("CONTEXT/RectTransform/Game Framework/UI Support/ピクセル単位で丸める")]
        private static void RoundRectTransformFromContext(MenuCommand command) {
            RoundRectTransform(command.context as RectTransform);
        }

        /// <summary>
        /// RectTransformコンテキストでScale正規化を実行
        /// </summary>
        [MenuItem("CONTEXT/RectTransform/Game Framework/UI Support/Scaleを1に正規化")]
        private static void NormalizeScaleFromContext(MenuCommand command) {
            NormalizeScale(command.context as RectTransform);
        }

        /// <summary>
        /// RectTransformコンテキストでアンカーフィットを実行
        /// </summary>
        [MenuItem("CONTEXT/RectTransform/Game Framework/UI Support/現在矩形にアンカーを合わせる")]
        private static void FitAnchorsToCurrentRectFromContext(MenuCommand command) {
            FitAnchorsToCurrentRect(command.context as RectTransform);
        }

        /// <summary>
        /// RectTransformコンテキストで親Stretch設定を実行
        /// </summary>
        [MenuItem("CONTEXT/RectTransform/Game Framework/UI Support/親Stretchアンカーを設定")]
        private static void SetParentStretchAnchorsFromContext(MenuCommand command) {
            SetParentStretchAnchors(command.context as RectTransform);
        }

        /// <summary>
        /// RectTransformコンテキストでアンカー正規化を実行
        /// </summary>
        [MenuItem("CONTEXT/RectTransform/Game Framework/UI Support/アンカーを正規化")]
        private static void NormalizeAnchorsFromContext(MenuCommand command) {
            NormalizeAnchors(command.context as RectTransform);
        }

        /// <summary>
        /// RectTransformコンテキストでLayout再計算を実行
        /// </summary>
        [MenuItem("CONTEXT/RectTransform/Game Framework/UI Support/Layoutを再計算")]
        private static void RebuildLayoutFromContext(MenuCommand command) {
            if (command.context is RectTransform rectTransform) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        /// <summary>
        /// ButtonコンテキストでNavigation None化を実行
        /// </summary>
        [MenuItem("CONTEXT/Button/Game Framework/UI Support/NavigationをNoneにする")]
        private static void SetButtonNavigationNoneFromContext(MenuCommand command) {
            SetButtonNavigationNone(command.context as Button);
        }
    }
}

