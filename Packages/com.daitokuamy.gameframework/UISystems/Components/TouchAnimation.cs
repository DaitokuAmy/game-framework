using GameFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework.UISystems {
    /// <summary>
    /// RectTransformのタッチに付与するアニメーション基底
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class TouchAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        [SerializeField, Tooltip("制御対象のRectTransform")]
        private RectTransform _target;
        [SerializeField, Tooltip("更新タイプ")]
        private UpdateType _updateType = UpdateType.LateUpdate;

        [Header("アニメーション")]
        [SerializeField, Tooltip("TouchDown時のアニメーション時間")]
        private float _downDuration = 0.1f;
        [SerializeField, Tooltip("TouchUp時のアニメーション時間")]
        private float _upDuration = 0.1f;

        private float _timer;
        private bool _isDown;

        /// <summary>TimeScale</summary>
        public float TimeScale { get; set; } = 1.0f;
        /// <summary>制御対象のボタン</summary>
        public RectTransform Target {
            get {
                if (_target == null) {
                    _target = (RectTransform)transform;
                }

                return _target;
            }
        }
        /// <summary>更新タイプ</summary>
        public UpdateType UpdateType {
            get => _updateType;
            set => _updateType = value;
        }
        
        /// <summary>
        /// TouchDown通知
        /// </summary>
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            _timer = _downDuration;
            _isDown = true;
        }

        /// <summary>
        /// TouchUp通知
        /// </summary>
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            _timer = _upDuration;
            _isDown = false;
        }

        /// <summary>
        /// アニメーションの適用
        /// </summary>
        /// <param name="isDown">TouchDown中か</param>
        /// <param name="ratio">補間割合</param>
        protected abstract void ApplyAnimation(bool isDown, float ratio);

        /// <summary>
        /// 手動更新
        /// </summary>
        public void ManualUpdate(float deltaTime) {
            if (_updateType == UpdateType.Manual) {
                deltaTime *= TimeScale;
                UpdateInternal(deltaTime);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (_updateType == UpdateType.Update) {
                var deltaTime = Time.deltaTime * TimeScale;
                UpdateInternal(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            if (_updateType == UpdateType.LateUpdate) {
                var deltaTime = Time.deltaTime * TimeScale;
                UpdateInternal(deltaTime);
            }
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            // 再生中のアニメーションがあれば完結させる
            if (_timer > 0.0f) {
                _timer = 0.0f;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void UpdateInternal(float deltaTime) {
            if (_timer >= 0.0f) {
                var ratio = _timer > float.Epsilon ? deltaTime / _timer : 1.0f;
                ratio = Mathf.Clamp01(ratio);
                ApplyAnimation(_isDown, ratio);

                if (_timer > 0.0f && _timer <= deltaTime) {
                    _timer = 0.0f;
                }
                else {
                    _timer -= deltaTime;
                }
            }
        }
    }
}