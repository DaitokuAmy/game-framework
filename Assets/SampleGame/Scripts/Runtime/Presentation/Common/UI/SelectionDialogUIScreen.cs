using System.Collections.Generic;
using GameFramework.Core;
using TMPro;
using UnityEngine;
using R3;

namespace SampleGame.Presentation {
    /// <summary>
    /// 汎用選択用ダイアログ用のスクリーン
    /// </summary>
    public class SelectionDialogUIScreen : DialogUIScreen {
        [SerializeField, Tooltip("タイトル設定用テキスト")]
        private TextMeshProUGUI _titleText;
        [SerializeField, Tooltip("項目ボタン用のボタンテンプレート")]
        private ButtonUIView _templateItemButtonView;
        [SerializeField, Tooltip("Closeボタン用のView")]
        private ButtonUIView _closeButtonView;

        private List<ButtonUIView> _itemButtonViews = new();
        private DisposableScope _setupScope = new();

        /// <summary>背面タッチをした時に返却するIndex</summary>
        protected override int BackgroundButtonIndex => -1;

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            // ボタンの監視
            _closeButtonView.ClickedSubject
                .TakeUntil(scope)
                .Subscribe(_ => Select(-1));
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="itemLabels">内容</param>
        /// <param name="closeText">閉じるボタンの表示テキスト</param>
        /// <param name="useBackgroundCancel">背面キャンセルボタンを使うか</param>
        public void Setup(string title, IReadOnlyList<string> itemLabels, string closeText, bool useBackgroundCancel = true) {
            SetText(_titleText, title);
            SetText(_closeButtonView, closeText);
            UseBackgroundButton = useBackgroundCancel;

            // 項目の初期化
            for (var i = 0; i < _itemButtonViews.Count; i++) {
                Destroy(_itemButtonViews[i].gameObject);
            }

            _setupScope.Clear();
            _itemButtonViews.Clear();

            _templateItemButtonView.gameObject.SetActive(true);
            for (var i = 0; i < itemLabels.Count; i++) {
                var index = i;
                var label = itemLabels[i];
                var view = InstantiateView(_templateItemButtonView, _templateItemButtonView.transform.parent);
                SetText(view, label);
                view.ClickedSubject
                    .TakeUntil(_setupScope)
                    .Subscribe(_ => { Select(index); });
                _itemButtonViews.Add(view);
            }

            _templateItemButtonView.gameObject.SetActive(false);
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