using UnityEngine;
using UnityEngine.UI;

namespace ThirdPersonEngine {
    /// <summary>
    /// イメージゲージ用のUIView
    /// </summary>
    public class ImageGaugeUIView : GaugeUIView {
        [Header("シリアライズ")]
        [SerializeField, Tooltip("前面のImage")]
        private Image _frontImage;
        [SerializeField, Tooltip("後面のImage(増加)")]
        private Image _backPositiveImage;
        [SerializeField, Tooltip("後面のImage(減少)")]
        private Image _backNegativeImage;

        /// <summary>
        /// 全面ゲージの値設定
        /// </summary>
        protected override void SetFrontGaugeValue(float targetValue, float rate) {
            SetGaugeValue(_frontImage, targetValue, rate);
        }
        
        /// <summary>
        /// 背面ゲージの値設定
        /// </summary>
        protected override void SetBackGaugeValue(float targetValue, float rate) {
            SetGaugeValue(_backPositiveImage, targetValue, rate);
            SetGaugeValue(_backNegativeImage, targetValue, rate);
        }

        /// <summary>
        /// アニメーションの開始通知
        /// </summary>
        protected override void OnStartAnimation(bool negative) {
            if (_backPositiveImage != null) {
                _backPositiveImage.enabled = !negative;
            }
            
            if (_backNegativeImage != null) {
                _backNegativeImage.enabled = negative;
            }
        }

        /// <summary>
        /// ゲージの値設定
        /// </summary>
        private void SetGaugeValue(Image image, float targetValue, float rate) {
            if (image == null) {
                return;
            }

            var targetRate = targetValue / MaxValue;
            image.fillAmount = Mathf.Lerp(image.fillAmount, targetRate, rate);
        }
    }
}
