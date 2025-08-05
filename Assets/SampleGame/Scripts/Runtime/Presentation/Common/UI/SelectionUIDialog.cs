using System.Collections.Generic;
using GameFramework;
using GameFramework.Core;
using GameFramework.UISystems;
using TMPro;
using UnityEngine;
using R3;
using ThirdPersonEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 汎用選択用ダイアログ
    /// </summary>
    public class SelectionUIDialog : AnimatableUIDialog {
        [SerializeField, Tooltip("タイトル設定用テキスト")]
        private TextMeshProUGUI _titleText;
        [SerializeField, Tooltip("項目ボタン用のボタンテンプレート")]
        private ButtonUIView _templateItemButtonView;
        [SerializeField, Tooltip("Closeボタン用のView")]
        private ButtonUIView _closeButtonView;
        
        private UIViewPool<ButtonUIView> _viewPool;

        /// <inheritdoc/>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            _viewPool = new UIViewPool<ButtonUIView>(_templateItemButtonView, template => InstantiateView(template, template.transform.parent))
                .RegisterTo(scope);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            // ボタンの監視
            _closeButtonView.ClickedSubject
                .TakeUntil(scope)
                .Subscribe(_ => Cancel());
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
            _viewPool.Clear();
            for (var i = 0; i < itemLabels.Count; i++) {
                var label = itemLabels[i];
                var view = _viewPool.Get((v, idx, scp) => {
                    v.ClickedSubject
                        .TakeUntil(scp)
                        .Subscribe(_ => { SelectIndex(idx); });
                });
                SetText(view, label);
            }
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