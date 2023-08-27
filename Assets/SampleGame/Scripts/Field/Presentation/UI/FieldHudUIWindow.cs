using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Field {
    /// <summary>
    /// FieldHud用のUIWindow
    /// </summary>
    public class FieldHudUIWindow : UIWindow {
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
        public async UniTask DailyDialogTestAsync() {
            await _modalContainer.Push("Daily")
                .ToUniTask();
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.C));
            await _modalContainer.Pop()
                .ToUniTask();
        }
    }
}