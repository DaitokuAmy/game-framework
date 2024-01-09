using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.UISystems {
    /// <summary>
    /// 色フェード制御用のUIView
    /// </summary>
    public class ColorFaderUIView : FaderUIView {
        [SerializeField, Tooltip("制御用イメージ")]
        private Image _image;

        /// <summary>
        /// 色の設定
        /// </summary>
        protected override void SetColor(Color color) {
            color.a = _image.color.a;
            _image.color = color;
        }

        /// <summary>
        /// 割合の反映
        /// </summary>
        protected override void ApplyRate(float rate) {
            var color = _image.color;
            color.a = rate;
            _image.color = color;
            _image.enabled = rate >= float.Epsilon;
        }
    }
}
