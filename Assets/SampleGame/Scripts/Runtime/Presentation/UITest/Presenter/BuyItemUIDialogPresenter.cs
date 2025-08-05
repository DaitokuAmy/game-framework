using GameFramework;
using GameFramework.Core;
using GameFramework.UISystems;
using R3;

namespace SampleGame.Presentation.UITest {
    /// <summary>
    /// BuyItemUIDialogのPresenter
    /// </summary>
    public class BuyItemUIDialogPresenter : UIScreenLogic<UITestBuyItemUIDialog> {
        /// <inheritdoc/>
        protected override void PreOpenInternal() {
            base.PreOpenInternal();
            
            // ItemIdを元に素材情報を取得
            var itemId = Screen.ItemId;
            // todo:値は仮
            Screen.SetInformation(1, 10, "テスト", null);
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            var dialogUIService = Services.Resolve<UIManager>().GetService<DialogUIService>();
            
            // 購入ボタン押下
            Screen.DecidedSubject
                .TakeUntil(scope)
                .Where(cnt => cnt > 0)
                .SubscribeAwait(async (cnt, ct) => {
                    var result = await dialogUIService.OpenMessageDialogAsync("確認", string.Empty, $"{cnt}個購入します\nよろしいですか？", "はい", "いいえ", true, ct);
                    if (result == MessageUIDialog.Result.Decide) {
                        Screen.Decide();
                    }
                });
        }
    }
}