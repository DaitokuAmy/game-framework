using GameFramework.UISystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame {
    /// <summary>
    /// 武器詳細用UIView
    /// </summary>
    public class WeaponDetailUIView : UIView {
        [SerializeField, Tooltip("アイコンイメージ")]
        private Image _iconImage;
        [SerializeField, Tooltip("詳細用テキスト")]
        private TextMeshProUGUI _descriptionText;

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
        /// <summary>説明文</summary>
        public string Description {
            get {
                if (_descriptionText == null) {
                    return "";
                }

                return _descriptionText.text;
            }
            set {
                if (_descriptionText == null) {
                    return;
                }

                _descriptionText.text = value;
            }
        }
    }
}
