using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Field {
    /// <summary>
    /// FieldHud用のUIService
    /// </summary>
    public class FieldHudUIService : UIService {
        [SerializeField, Tooltip("戻るボタン")]
        private ButtonUIView _backButton;
        [SerializeField, Tooltip("ヘッダー")]
        private FieldHeaderUIView _headerUIView;
        [SerializeField, Tooltip("フッター")]
        private FieldFooterUIView _footerUIView;
        [SerializeField, Tooltip("モーダル用のコンテナ")]
        private UIModalContainer _modalContainer;

        /// <summary>戻るボタン</summary>
        public ButtonUIView BackButton => _backButton;
        /// <summary>ヘッダー</summary>
        public FieldHeaderUIView HeaderUIView => _headerUIView;
        /// <summary>フッター</summary>
        public FieldFooterUIView FooterUIView => _footerUIView;

        /// <summary>
        /// デイリーダイアログを開く
        /// </summary>
        public UniTask<UIScreen> OpenDailyDialogAsync() {
            return _modalContainer.Push("Daily")
                .ToUniTask();
        }

        /// <summary>
        /// ダイアログを一つ閉じる
        /// </summary>
        public UniTask BackDialogAsync() {
            return _modalContainer.Pop()
                .ToUniTask();
        }
    }
}