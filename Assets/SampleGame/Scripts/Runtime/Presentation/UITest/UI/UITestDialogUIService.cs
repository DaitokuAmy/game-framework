using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation.UITest {
    /// <summary>
    /// UITestDialog用のUIService
    /// </summary>
    public class UITestDialogUIService : UIService {
        [SerializeField, Tooltip("ダイアログコンテナ")]
        private UIDialogContainer _dialogContainer;

        /// <summary>
        /// 購入ダイアログ用のハンドラーを設定
        /// </summary>
        public void SetBuyItemDialogHandler(Func<IUIScreenHandler> createHandlerFunc) {
            _dialogContainer.SetHandler("BuyItem", createHandlerFunc);
        }

        /// <summary>
        /// 購入ダイアログを開く
        /// </summary>
        public async UniTask<UITestBuyItemUIDialog.Result> OpenBuyItemDialogAsync(int itemId, CancellationToken ct) {
            return await _dialogContainer.OpenDialog<UITestBuyItemUIDialog, UITestBuyItemUIDialog.Result>("BuyItem", dialog => { dialog.Setup(itemId); });
        }
    }
}