using System;
using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// セーフエリア対応
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour {
        /// <summary>
        /// 適用対象の辺
        /// </summary>
        [Flags]
        private enum Edges {
            Left = 1 << 0,
            Right = 1 << 1,
            Bottom = 1 << 2,
            Top = 1 << 3
        }

        [SerializeField, Tooltip("適用対象の辺")]
        private Edges _edges = Edges.Left | Edges.Right | Edges.Top | Edges.Bottom;

        // 制御対象のRectTransform
        private RectTransform _rectTransform;
        // 最後に適用したSafeArea
        private Rect _lastSafeArea;
        // 最後に適用した解像度
        private Vector2Int _lastResolution;
        // 更新フラグ
        private bool _dirty;

#if UNITY_EDITOR
        // RectTransformの操作を無効にするため
        private DrivenRectTransformTracker _rectTransformTracker = new();
#endif

        /// <summary>制御するRectTransform</summary>
        private RectTransform RectTransform {
            get {
                if (_rectTransform == null) {
                    _rectTransform = (RectTransform)transform;
                }

                return _rectTransform;
            }
        }

        /// <summary>
        /// SafeAreaの反映
        /// </summary>
        private void Apply(bool force = false) {
            var safeArea = Screen.safeArea;
            var resolution = new Vector2Int(Screen.width, Screen.height);

            if (resolution.x == 0 || resolution.y == 0) {
                return;
            }

            if (!force) {
                if (_lastSafeArea == safeArea && _lastResolution == resolution) {
                    return;
                }
            }

            _lastSafeArea = safeArea;
            _lastResolution = resolution;

            var trans = RectTransform;
            var anchorMin = new Vector2(safeArea.xMin / resolution.x, safeArea.yMin / resolution.y);
            var anchorMax = new Vector2(safeArea.xMax / resolution.x, safeArea.yMax / resolution.y);

            if ((_edges & Edges.Left) == 0) {
                anchorMin.x = 0.0f;
            }

            if ((_edges & Edges.Right) == 0) {
                anchorMax.x = 1.0f;
            }

            if ((_edges & Edges.Bottom) == 0) {
                anchorMin.y = 0.0f;
            }

            if ((_edges & Edges.Top) == 0) {
                anchorMax.y = 1.0f;
            }

            trans.anchoredPosition = Vector2.zero;
            trans.sizeDelta = Vector2.zero;
            trans.anchorMin = anchorMin;
            trans.anchorMax = anchorMax;
        }

        /// <summary>
        /// スクリプト変更時処理
        /// </summary>
        private void OnValidate() {
            _dirty = true;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            Apply(_dirty);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _dirty = true;
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
#if UNITY_EDITOR
            _rectTransformTracker.Clear();
#endif
        }
    }
}