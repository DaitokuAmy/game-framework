using GameFramework.Core;
using TMPro;
using UnityEngine;
using UniRx;

namespace SampleGame.Presentation {
    /// <summary>
    /// 汎用メッセージダイアログ用のスクリーン
    /// </summary>
    public class MessageDialogUIScreen : DialogUIScreen {
        /// <summary>
        /// 結果のIndex
        /// </summary>
        public enum Result {
            Invalid = -1,
            Cancel = 0,
            Decide = 1,
        }
        
        [SerializeField, Tooltip("タイトル設定用テキスト")]
        private TextMeshProUGUI _titleText;
        [SerializeField, Tooltip("サブタイトル設定用テキスト")]
        private TextMeshProUGUI _subTitleText;
        [SerializeField, Tooltip("内容設定用テキスト")]
        private TextMeshProUGUI _contentText;
        [SerializeField, Tooltip("Decideボタン用のView")]
        private ButtonUIView _decideButtonView;
        [SerializeField, Tooltip("Cancelボタン用のView")]
        private ButtonUIView _cancelButtonView;
        
        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);
            
            // ボタンの監視
            _decideButtonView.ClickedSubject
                .TakeUntil(scope)
                .Subscribe(_ => Select((int)Result.Decide));
            
            _cancelButtonView.ClickedSubject
                .TakeUntil(scope)
                .Subscribe(_ => Select((int)Result.Cancel));
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="subTitle">サブタイトル</param>
        /// <param name="content">内容</param>
        /// <param name="decideText">決定ボタンの表示テキスト</param>
        /// <param name="cancelText">キャンセルボタンの表示テキスト</param>
        /// <param name="useBackgroundCancel">背面キャンセルボタンを使うか</param>
        public void Setup(string title, string subTitle, string content, string decideText, string cancelText, bool useBackgroundCancel = true) {
            SetText(_titleText, title);
            SetText(_subTitleText, subTitle);
            SetText(_contentText, content);
            SetText(_decideButtonView, decideText);
            SetText(_cancelButtonView, cancelText);
            UseBackgroundButton = useBackgroundCancel;
        }

        /// <summary>
        /// テキストの設定
        /// </summary>
        private void SetText(TextMeshProUGUI text, string content) {
            if (text == null) {
                return;
            }

            if (string.IsNullOrEmpty(content)) {
                text.enabled = false;
            }
            else {
                text.enabled = true;
                text.text = content;
            }
        }

        /// <summary>
        /// テキストの設定
        /// </summary>
        private void SetText(ButtonUIView buttonView, string content) {
            if (buttonView == null) {
                return;
            }

            if (string.IsNullOrEmpty(content)) {
                buttonView.gameObject.SetActive(false);
            }
            else {
                buttonView.gameObject.SetActive(true);
                buttonView.Text = content;
            }
        }
    }
}
