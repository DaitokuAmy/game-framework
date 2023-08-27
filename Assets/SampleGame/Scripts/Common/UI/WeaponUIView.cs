using UnityEngine;
using UnityEngine.UI;

namespace SampleGame {
    /// <summary>
    /// 武器用UIView
    /// </summary>
    public class WeaponUIView : ButtonUIView {
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
