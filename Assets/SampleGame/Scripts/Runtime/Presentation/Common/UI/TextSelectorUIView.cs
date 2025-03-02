using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Text選択用のUIView
    /// </summary>
    public class TextSelectorUIView : SelectorUIView {
        [SerializeField, Tooltip("ラベル表示用のText")]
        private TextMeshProUGUI _labelText;
        
        private readonly List<string> _labels = new();

        /// <summary>選択中のラベル</summary>
        public string SelectedLabel {
            get {
                if (SelectedIndex < 0) {
                    return "";
                }

                return _labels[SelectedIndex];
            }
        }

        /// <summary>
        /// 選択Indexの変更通知(_selectedIndexの変更より前)
        /// </summary>
        /// <param name="index">変更されたIndex(負の値は無効値)</param>
        protected override void OnChangedIndex(int index) {
            if (_labelText != null) {
                if (index >= 0 && index < _labels.Count) {
                    _labelText.text = _labels[index];
                }
                else {
                    _labelText.text = "";
                }
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(string[] labels) {
            _labels.Clear();
            _labels.AddRange(labels);
            ItemCount = 0;
            ItemCount = _labels.Count;
        }
    }
}
