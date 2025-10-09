using GameFramework.UISystems;
using UnityEngine;
using UnityEngine.UI;

namespace ThirdPersonEngine {
    /// <summary>
    /// スライダーゲージ用のUIView
    /// </summary>
    public class SliderGaugeUIView : GaugeUIView {
        [Header("シリアライズ")]
        [SerializeField, Tooltip("前面のSlider")]
        private Slider _frontSlider;
        [SerializeField, Tooltip("後面のSlider(加算)")]
        private Slider _backPositiveSlider;
        [SerializeField, Tooltip("後面のSlider(減少)")]
        private Slider _backNegativeSlider;
        
        /// <inheritdoc/>
        protected override bool IsFrontAnimationOnly => _backPositiveSlider == null && _backNegativeSlider == null;

        /// <summary>
        /// 全面ゲージの値設定
        /// </summary>
        protected override void SetFrontGaugeValue(float targetValue, float rate) {
            SetGaugeValue(_frontSlider, targetValue, rate);
        }
        
        /// <summary>
        /// 背面ゲージの値設定
        /// </summary>
        protected override void SetBackGaugeValue(float targetValue, float rate) {
            SetGaugeValue(_backPositiveSlider, targetValue, rate);
            SetGaugeValue(_backNegativeSlider, targetValue, rate);
        }

        /// <summary>
        /// 最大値の変更通知
        /// </summary>
        protected override void OnChangedMaxValue(float maxValue) {
            if (_frontSlider != null) {
                _frontSlider.maxValue = maxValue;
            }

            if (_backPositiveSlider != null) {
                _backPositiveSlider.maxValue = maxValue;
            }

            if (_backNegativeSlider != null) {
                _backNegativeSlider.maxValue = maxValue;
            }
        }

        /// <summary>
        /// アニメーション開始通知
        /// </summary>
        /// <param name="negative">減少か</param>
        protected override void OnStartAnimation(bool negative) {
            if (_backPositiveSlider != null) {
                _backPositiveSlider.gameObject.SetActive(!negative);
            }
            
            if (_backNegativeSlider != null) {
                _backNegativeSlider.gameObject.SetActive(negative);
            }
        }

        /// <summary>
        /// ゲージの値設定
        /// </summary>
        private void SetGaugeValue(Slider slider, float targetValue, float rate) {
            if (slider == null) {
                return;
            }

            slider.value = Mathf.Lerp(slider.value, targetValue, rate);
        }
    }
}