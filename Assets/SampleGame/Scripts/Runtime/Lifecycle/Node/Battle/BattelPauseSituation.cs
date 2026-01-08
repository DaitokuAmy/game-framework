using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using GameFramework.UISystems;
using SampleGame.Application;
using SampleGame.Presentation;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle用の一時停止中Situation
    /// </summary>
    public class BattlePauseSituation : ScreenNode {
        /// <inheritdoc/>
        protected override void Activate(TransitionHandle<INavNode> handle, IScope scope) {
            base.Activate(handle, scope);

            var situationService = ServiceResolver.Resolve<IAppNavigator>();
            var uiManager = ServiceResolver.Resolve<UIManager>();
            var dialogUIService = uiManager.GetService<DialogUIService>();

            var itemLabels = new[] { "タイトルに戻る", "メッセージテスト" };
            dialogUIService.OpenSelectionDialogAsync("ポーズメニュー", itemLabels, useBackgroundCancel: true, ct: scope.Token)
                .ContinueWith(result => {
                    switch (result) {
                        case 0:
                            situationService.TransitionTitleTop();
                            break;
                        case 1:
                            dialogUIService.OpenMessageDialogAsync("テスト", "メッセージサブ", "メッセージ内容", useBackgroundCancel: true, ct: scope.Token)
                                .ContinueWith(r => { SystemLog.Info($"MessageResult:{r}"); });
                            situationService.Back();
                            break;
                        default:
                            situationService.Back();
                            break;
                    }
                })
                .Forget();
        }
    }
}