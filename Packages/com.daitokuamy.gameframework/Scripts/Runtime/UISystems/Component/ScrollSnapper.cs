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
        [SerializeField, Tooltip("慣性の影響割合")]
        private float _inertiaInfluenceRate = 1.0f;
        [SerializeField, Tooltip("スナップにかかる時間")]
        private float _snapDuration = 0.2f;
        [SerializeField, Tooltip("スナップ位置を中央に揃えるか（falseなら左上基準）")]
        private bool _centerAlign = true;

        private ScrollRect _scrollRect;
        private bool _isDragging;
        private float _timeScale = 1.0f;
        private float _inertiaOffsetIndex;
        private float? _targetIndex;

        /// <summary>現在カレント扱いとなっているIndex(小数あり)</summary>
        public float CurrentIndex { get; private set; } = -1.0f;
        /// <summary>タイムスケール</summary>
        public float TimeScale {
            get => _timeScale;
            set => _timeScale = Mathf.Max(0.0f, _timeScale);
        }

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
        /// ターゲットIndexの設定
        /// </summary>
        public void SetTargetIndex(float index, bool immediate = true) {
            _targetIndex = index;
            if (immediate) {
                Snap(0.1f, true);
            }
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
            var deltaTime = Time.deltaTime * TimeScale;
            
            // CurrentIndexの更新
            UpdateCurrentIndex(deltaTime);

            // スナップ処理
            if (ShouldSnap()) {
                Snap(deltaTime);
            }
        }

        /// <summary>
        /// スナップすべきか
        /// </summary>
        private bool ShouldSnap() {
            if (_targetIndex.HasValue) {
                return true;
            }
            
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
        private void Snap(float deltaTime, bool immediate = false) {
            if (Content.childCount == 0) {
                return;
            }

            var startPosition = Content.anchoredPosition;
            var targetPosition = startPosition;
            var snapPosition = GetSnapPosition();

            // 目標位置を算出
            var index = Mathf.RoundToInt(_targetIndex ?? CurrentIndex + _inertiaOffsetIndex);
            var snapTarget = GetElementPosition(index);
            var delta = snapPosition - snapTarget;
            if (IsHorizontal) {
                targetPosition.x += delta;
            }
            else {
                targetPosition.y -= delta;
            }

            // 位置補正
            if (immediate) {
                Content.anchoredPosition = targetPosition;
            }
            else {
                var t = _snapDuration > float.Epsilon ? 1.0f - Mathf.Exp(-1.0f / _snapDuration * deltaTime) : 1.0f;
                Content.anchoredPosition = Vector2.Lerp(Content.anchoredPosition, targetPosition, t);
            }

            // 速度をリセット
            _scrollRect.velocity = Vector2.zero;
            
            // ターゲットIndexがある場合、到着したらnullに戻す
            if (_targetIndex.HasValue) {
                var current = IsHorizontal ? Content.anchoredPosition.x : Content.anchoredPosition.y;
                var target = IsHorizontal ? targetPosition.x : targetPosition.y;
                if (Mathf.Approximately(current, target)) {
                    _targetIndex = null;
                }
            }
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
        private void UpdateCurrentIndex(float deltaTime) {
            if (Content.childCount == 0) {
                CurrentIndex = -1.0f;
                _inertiaOffsetIndex = 0.0f;
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

            // 移動速度を元に慣性オフセットを算出
            if (deltaTime > 0.001f) {
                if (_scrollRect.inertia) {
                    var speed = IsHorizontal ? _scrollRect.velocity.x : _scrollRect.velocity.y;
                    var offset = EstimateInertiaOffset(speed, _scrollRect.decelerationRate, deltaTime);
                    _inertiaOffsetIndex = Mathf.Clamp(offset / unitSize * _inertiaInfluenceRate, -1, 1);
                }
                else {
                    _inertiaOffsetIndex = 0.0f;
                }
            }
        }

        /// <summary>
        /// 自動停止するまでの距離を求める計算
        /// </summary>
        /// <param name="speed">現在速度</param>
        /// <param name="decelerationRate">減衰割合</param>
        /// <param name="deltaTime">変位時間</param>
        /// <param name="stopSpeed">停止とみなす速度</param>
        private float EstimateInertiaOffset(float speed, float decelerationRate, float deltaTime, float stopSpeed = 0.01f) {
            if (deltaTime <= 0.0f || decelerationRate >= 1.0f) {
                return speed >= 0.0f ? float.MaxValue : float.MinValue;
            }

            if (Mathf.Approximately(speed, 0.0f) || decelerationRate <= 0.0f) {
                return 0.0f;
            }

            var currentSpeed = speed;
            var offset = 0.0f;
            var decayFactor = Mathf.Pow(decelerationRate, deltaTime);

            while (Mathf.Abs(currentSpeed) > stopSpeed) {
                offset += currentSpeed * deltaTime;
                currentSpeed *= decayFactor;
            }

            return offset;
        }
    }
}