using GameFramework.UISystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// ゲージ用のUIViewの基底
    /// </summary>
    public abstract class GaugeUIView : UIView {
        [Header("アニメーション")]
        [SerializeField, Tooltip("先に動くアニメーション時間")]
        private float _fastChangeDuration = 0.0f;
        [SerializeField, Tooltip("後で動くアニメーション時間")]
        private float _nextChangeDuration = 1.0f;
        [SerializeField, Tooltip("変化遅延時間")]
        private float _ChangeDelay = 0.5f;

        private float _fastTimer;
        private float _nextTimer;
        private bool _negative;
        
        /// <summary>ゲージ最大値</summary>
        public float MaxValue { get; private set; }
        /// <summary>現在のゲージ値</summary>
        public float Value { get; private set; }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            base.LateUpdateInternal(deltaTime);
            
            // アニメーション
            if (_fastTimer >= 0.0f) {
                var rate = _fastTimer > float.Epsilon ? Mathf.Min(1.0f, deltaTime / _fastTimer) : 1.0f;
                // 値が減った場合、frontが先に動く
                if (_negative) {
                    SetFrontGaugeValue(Value, rate);
                }
                else {
                    SetBackGaugeValue(Value, rate);
                }
                
                _fastTimer -= deltaTime;
            }

            if (_nextTimer >= 0.0f) {
                var rate = _nextTimer > _nextChangeDuration ? 0.0f : _nextTimer > float.Epsilon ? Mathf.Min(1.0f, deltaTime / _nextTimer) : 1.0f;
                // 値が増えた場合、Frontが後に動く
                if (_negative) {
                    SetBackGaugeValue(Value, rate);
                }
                else {
                    SetFrontGaugeValue(Value, rate);
                }
                _nextTimer -= deltaTime;
            }
        }

        /// <summary>
        /// FrontGaugeの値反映
        /// </summary>
        /// <param name="targetValue">目的割合</param>
        /// <param name="rate">補間割合</param>
        protected abstract void SetFrontGaugeValue(float targetValue, float rate);

        /// <summary>
        /// BackGaugeの値反映
        /// </summary>
        /// <param name="targetValue">目的値</param>
        /// <param name="rate">補間割合</param>
        protected abstract void SetBackGaugeValue(float targetValue, float rate);
        
        /// <summary>
        /// 最大値の更新通知
        /// </summary>
        protected virtual void OnChangedMaxValue(float maxValue) {}
        
        /// <summary>
        /// アニメーション開始通知
        /// </summary>
        /// <param name="negative">値が減る方向に変化したか</param>
        protected virtual void OnStartAnimation(bool negative) {}

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="initValue">初期値</param>
        /// <param name="maxValue">ゲージの最大値</param>
        public void Setup(float initValue, float maxValue) {
            Value = Mathf.Clamp(initValue, 0.0f, maxValue);
            MaxValue = maxValue;
            OnChangedMaxValue(MaxValue);
            
            // 値の初期化
            SetFrontGaugeValue(Value, 1.0f);
            SetBackGaugeValue(Value, 1.0f);
        }

        /// <summary>
        /// ゲージの変更
        /// </summary>
        /// <param name="value">変化後の値</param>
        /// <param name="immediate">即時変化させるか</param>
        public void Change(int value, bool immediate = false) {
            // 現在の値を即時反映
            SetFrontGaugeValue(Value, 1.0f);
            SetBackGaugeValue(Value, 1.0f);

            _negative = value < Value;
            
            // 次の目的値を指定
            Value = Mathf.Clamp(value, 0.0f, MaxValue);

            // アニメーション用のタイマーを設定
            if (immediate) {
                _fastTimer = 0.0f;
                _nextTimer = 0.0f;
            }
            else {
                _fastTimer = _fastChangeDuration;
                _nextTimer = _nextChangeDuration + _ChangeDelay;
                // 通知
                OnStartAnimation(_negative);
            }
        }
    }
}
