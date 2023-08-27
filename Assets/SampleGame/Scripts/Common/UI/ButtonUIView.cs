using System;
using GameFramework.UISystems;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame {
    /// <summary>
    /// ボタン用UIView
    /// </summary>
    public class ButtonUIView : UIView {
        [SerializeField, Tooltip("決定ボタン")]
        private Button _button;
        [SerializeField, Tooltip("表示テキスト")]
        private TextMeshProUGUI _text;

        /// <summary>表示テキスト</summary>
        public string Text {
            get {
                if (_text == null) {
                    return "";
                }

                return _text.text;
            }
            set {
                if (_text == null) {
                    return;
                }

                _text.text = value;
            }
        }

        /// <summary>クリック通知</summary>
        public IObservable<Unit> OnClickSubject => _button != null ? _button.OnClickAsObservable() : Observable.Empty<Unit>();
    }
}
