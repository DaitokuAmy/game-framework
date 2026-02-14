using System;
using UnityEditor;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// 2分割レイアウト描画ヘルパー
    /// </summary>
    public sealed class SplitView {
        /// <summary>
        /// 分割方向
        /// </summary>
        public enum Direction {
            Horizontal,
            Vertical,
        }

        private readonly Direction _direction;
        private readonly float _splitterSize;

        private bool _dragging;
        private float _ratio;
        private float _minPrimary;
        private float _minSecondary;

        /// <summary>分割比率（0..1）</summary>
        public float Ratio {
            get => _ratio;
            set => _ratio = Mathf.Clamp01(value);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SplitView(Direction direction = Direction.Horizontal, float defaultRatio = 0.5f, float splitterSize = 4.0f) {
            _direction = direction;
            _splitterSize = splitterSize;
            _ratio = Mathf.Clamp01(defaultRatio);
        }

        /// <summary>
        /// 最小サイズを設定
        /// </summary>
        public SplitView SetMinSize(float primary, float secondary) {
            _minPrimary = Mathf.Max(0.0f, primary);
            _minSecondary = Mathf.Max(0.0f, secondary);
            return this;
        }

        /// <summary>
        /// 分割描画
        /// </summary>
        public void OnGUI(Rect rect, Action<Rect> drawPrimary, Action<Rect> drawSecondary) {
            var total = _direction == Direction.Horizontal ? rect.width : rect.height;
            var rectPos = _direction == Direction.Horizontal ? rect.x : rect.y;

            var ratioMin = total > 0.0f ? _minPrimary / total : 0.0f;
            var ratioMax = total > 0.0f ? 1.0f - (_minSecondary / total) : 1.0f;
            _ratio = Mathf.Clamp(_ratio, ratioMin, ratioMax);

            var splitPos = Mathf.Lerp(rectPos, rectPos + total, _ratio);

            var primaryRect = rect;
            var secondaryRect = rect;
            var splitterRect = rect;

            if (_direction == Direction.Horizontal) {
                primaryRect.width = Mathf.Max(0.0f, splitPos - rect.x);
                splitterRect.x = splitPos - (_splitterSize * 0.5f);
                splitterRect.width = _splitterSize;
                secondaryRect.x = splitterRect.xMax;
                secondaryRect.width = Mathf.Max(0.0f, rect.xMax - secondaryRect.x);
                EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
            }
            else {
                primaryRect.height = Mathf.Max(0.0f, splitPos - rect.y);
                splitterRect.y = splitPos - (_splitterSize * 0.5f);
                splitterRect.height = _splitterSize;
                secondaryRect.y = splitterRect.yMax;
                secondaryRect.height = Mathf.Max(0.0f, rect.yMax - secondaryRect.y);
                EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);
            }

            HandleDrag(splitterRect, total, rectPos, ratioMin, ratioMax);

            drawPrimary?.Invoke(primaryRect);

            EditorGUI.DrawRect(splitterRect, EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.68f, 0.68f, 0.68f));

            drawSecondary?.Invoke(secondaryRect);
        }

        /// <summary>
        /// 分割描画
        /// </summary>
        public void OnGUILayout(Action<Rect> drawPrimary, Action<Rect> drawSecondary, params GUILayoutOption[] options) {
            var rect = GUILayoutUtility.GetRect(1.0f, 100000.0f, 1.0f, 100000.0f, options);
            OnGUI(rect, drawPrimary, drawSecondary);
        }

        /// <summary>
        /// ドラッグ操作を処理
        /// </summary>
        private void HandleDrag(Rect splitterRect, float total, float rectPos, float ratioMin, float ratioMax) {
            var evt = Event.current;
            switch (evt.rawType) {
                case EventType.MouseDown:
                    if (splitterRect.Contains(evt.mousePosition) && evt.button == 0) {
                        _dragging = true;
                        evt.Use();
                    }

                    break;
                case EventType.MouseDrag:
                    if (_dragging) {
                        var pos = _direction == Direction.Horizontal ? evt.mousePosition.x : evt.mousePosition.y;
                        _ratio = total > 0.0f ? Mathf.Clamp((pos - rectPos) / total, ratioMin, ratioMax) : _ratio;
                        if (float.IsNaN(_ratio) || float.IsInfinity(_ratio)) {
                            _ratio = 0.5f;
                        }

                        evt.Use();
                    }

                    break;
                case EventType.MouseUp:
                    if (_dragging && evt.button == 0) {
                        _dragging = false;
                        evt.Use();
                    }

                    break;
            }
        }
    }
}
