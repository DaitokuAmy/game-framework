using UnityEngine;
using UnityEngine.UI;

namespace SampleGame {
    /// <summary>
    /// 防具用UIView
    /// </summary>
    public class ArmorUIView : ButtonUIView {
        [SerializeField, Tooltip("アイコンイメージ")]
        private Image _iconImage;

        /// <summary>アイコン</summary>
        public Sprite IconSprite {
            get {
                if (_iconImage == null) {
                    return null;
                }

                return _iconImage.overrideSprite;
            }
            set {
                if (_iconImage == null) {
                    return;
                }

                _iconImage.overrideSprite = value;
            }
        }
    }
}
