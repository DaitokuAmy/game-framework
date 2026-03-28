using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.UISystem {
    /// <summary>
    /// 色フェード制御用のUIView
    /// </summary>
    public class ColorFaderUIView : FaderUIView {
        [SerializeField, Tooltip("制御用イメージ")]
        private Image _image;

        /// <inheritdoc/>
        protected override void SetColor(Color color) {
            color.a = _image.color.a;
            _image.color = color;
        }

        /// <inheritdoc/>
        protected override void ApplyRate(float rate) {
            var color = _image.color;
            color.a = rate;
            _image.color = color;
            _image.enabled = rate >= float.Epsilon;
        }
    }
}
