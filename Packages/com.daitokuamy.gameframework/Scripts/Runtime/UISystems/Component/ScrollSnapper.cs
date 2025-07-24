using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameFramework.UISystems {
    /// <summary>
    /// ScrollRectを固定位置にスナップさせるための機能
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollSnapper : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
        [SerializeField, Tooltip("要素のサイズ（自動計算しない場合に指定）")]
        private float _itemSize = -1.0f;
        [SerializeField, Tooltip("要素リストのパディング（LayoutGroupがある場合自動設定）")]
        private RectOffset _padding;
        [SerializeField, Tooltip("要素間のスペース（LayoutGroupがある場合自動設定）")]
        private float _spacing = 0.0f;
        [SerializeField, Tooltip("スナップ開始時の速度閾値")]
        private float _snapSpeedThreshold = 100.0f;
        [SerializeField, Tooltip("スナップバネの強さ")]
        private float _snapSpring = 0.1f;
        [SerializeField, Tooltip("スナップ位置を中央に揃えるか（falseなら左上基準）")]
        private bool _centerAlign = true;

        private ScrollRect _scrollRect;
        private bool _isDragging;

        /// <summary>現在カレント扱いとなっているIndex(小数あり)</summary>
        public float CurrentIndex { get; private set; } = -1.0f;

        /// <summary>要素を入れるRectTransform</summary>
        private RectTransform Content => _scrollRect.content;
        /// <summary>要素をクリップするRectTransform</summary>
        private RectTransform Viewport => _scrollRect.viewport != null ? _scrollRect.viewport : (RectTransform)_scrollRect.transform;
        /// <summary>水平リストか</summary>
        private bool IsHorizontal => _scrollRect.horizontal;

        /// <inheritdoc/>
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            _isDragging = true;
        }

        /// <inheritdoc/>
        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            _isDragging = false;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _scrollRect = GetComponent<ScrollRect>();

            var layoutGroup = Content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null) {
                _spacing = layoutGroup.spacing;
                _padding = layoutGroup.padding;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            UpdateCurrentIndex();

            // スナップ処理
            if (ShouldSnap()) {
                Snap();
            }
        }

        /// <summary>
        /// スナップすべきか
        /// </summary>
        private bool ShouldSnap() {
            if (_isDragging) {
                return false;
            }

            if (Mathf.Abs(CurrentIndex % 1.0f) <= 0.001f) {
                return false;
            }

            var speed = IsHorizontal ? _scrollRect.velocity.x : _scrollRect.velocity.y;
            return speed * speed <= _snapSpeedThreshold * _snapSpeedThreshold;
        }

        /// <summary>
        /// スナップ処理
        /// </summary>
        private void Snap() {
            if (Content.childCount == 0) {
                return;
            }

            var startPosition = Content.anchoredPosition;
            var targetPosition = startPosition;
            var snapPosition = GetSnapPosition();

            // 目標位置を算出
            var index = Mathf.RoundToInt(CurrentIndex);
            var snapTarget = GetElementPosition(index);
            var delta = snapPosition - snapTarget;
            if (IsHorizontal) {
                targetPosition.x += delta;
            }
            else {
                targetPosition.y -= delta;
            }

            // 位置補正
            Content.anchoredPosition = Vector2.Lerp(Content.anchoredPosition, targetPosition, _snapSpring);
            
            // 速度をリセット
            _scrollRect.velocity = Vector2.zero;
        }

        /// <summary>
        /// スナップ位置を取得
        /// </summary>
        private float GetSnapPosition() {
            if (_centerAlign) {
                var viewport = Viewport;
                return IsHorizontal
                    ? -Content.anchoredPosition.x + viewport.rect.width * 0.5f
                    : Content.anchoredPosition.y + viewport.rect.height * 0.5f;
            }

            return IsHorizontal
                ? -Content.anchoredPosition.x + _spacing
                : Content.anchoredPosition.y + _spacing;
        }

        /// <summary>
        /// 要素の基準位置を取得
        /// </summary>
        private float GetElementPosition(int index) {
            var pos = index * (_itemSize + _spacing) + (IsHorizontal ? _padding.left : _padding.top);
            if (_centerAlign) {
                pos += _itemSize * 0.5f;
            }

            return pos;
        }

        /// <summary>
        /// カレントIndexを更新
        /// </summary>
        private void UpdateCurrentIndex() {
            if (Content.childCount == 0) {
                CurrentIndex = -1.0f;
                return;
            }

            // 要素サイズが未指定の場合、自動取得
            if (_itemSize <= 0.0f) {
                var first = Content.GetChild(0) as RectTransform;
                if (first != null) {
                    _itemSize = IsHorizontal ? first.rect.width : first.rect.height;
                }
            }

            // スクロール位置を元にスナップ対象となるIndexを小数点単位で求める
            var snapPosition = GetSnapPosition();
            var unitSize = _itemSize + _spacing;
            var elementPos = GetElementPosition(0);
            var diff = snapPosition - elementPos;
            CurrentIndex = diff / unitSize;
        }
    }
}