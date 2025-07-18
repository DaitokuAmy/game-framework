using System;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace SituationTreeSample {
    /// <summary>
    /// 選択項目用のView
    /// </summary>
    public class SelectionItemView : MonoBehaviour {
        [SerializeField, Tooltip("色を設定するImage")]
        private Image _colorImage;
        [SerializeField, Tooltip("ボタン")]
        private Button _button;
        [SerializeField, Tooltip("テキスト")]
        private TextMeshProUGUI _text;

        [SerializeField, Tooltip("カレント色")]
        private Color _currentColor;
        [SerializeField, Tooltip("デフォルト色")]
        private Color _defaultColor;

        /// <summary>表示テキスト</summary>
        public string Text {
            get => _text.text;
            set => _text.text = value;
        }
        /// <summary>カレントか</summary>
        public bool IsCurrent {
            set => _colorImage.color = value ? _currentColor : _defaultColor;
        }
        
        /// <summary>押下通知</summary>
        public Observable<Unit> ClickedSubject => _button.OnClickAsObservable();
    }
}