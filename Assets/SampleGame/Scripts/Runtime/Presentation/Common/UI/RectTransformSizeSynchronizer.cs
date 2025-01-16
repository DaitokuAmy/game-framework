using System;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// RectTransformのサイズを同期するコンポーネント
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformSizeSynchronizer : MonoBehaviour {
        /// <summary>
        /// 同期する軸
        /// </summary>
        [Flags]
        private enum SynchronizeAxis {
            Horizontal = 1 << 0,
            Vertical = 1 << 1,
        }
        
        [SerializeField, Tooltip("同期元のRectTransform")]
        private RectTransform _originRectTransform;

        [SerializeField, Tooltip("同期する軸")]
        private SynchronizeAxis _synchronizeAxis = SynchronizeAxis.Horizontal | SynchronizeAxis.Vertical;

        [SerializeField, Tooltip("サイズのオフセット")]
        private Vector2 _offset;
        
        // 制御対象のRectTransform
        private RectTransform _rectTransform;

#if UNITY_EDITOR
        // RectTransformの操作を無効にするため
        private DrivenRectTransformTracker _rectTransformTracker = new();
#endif

        private void Synchronize() {
            if (_rectTransform == null) {
                _rectTransform = (RectTransform)transform;
            }
            
#if UNITY_EDITOR
            _rectTransformTracker.Clear();
#endif

            if (_originRectTransform == null) {
                return;
            }

#if UNITY_EDITOR
            if ((_synchronizeAxis & SynchronizeAxis.Horizontal) != 0) {
                _rectTransformTracker.Add(this, _rectTransform, DrivenTransformProperties.SizeDeltaX);
            }

            if ((_synchronizeAxis & SynchronizeAxis.Vertical) != 0) {
                _rectTransformTracker.Add(this, _rectTransform, DrivenTransformProperties.SizeDeltaY);
            }
#endif

            var rect = _originRectTransform.rect;

            if ((_synchronizeAxis & SynchronizeAxis.Horizontal) != 0) {
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width + _offset.x);
            }

            if ((_synchronizeAxis & SynchronizeAxis.Vertical) != 0) {
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height + _offset.y);
            }
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
#if UNITY_EDITOR
            _rectTransformTracker.Clear();
#endif
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            Synchronize();
        }
    }
}