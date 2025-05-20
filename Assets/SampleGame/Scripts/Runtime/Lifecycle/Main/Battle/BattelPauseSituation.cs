using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle用の一時停止中Situation
    /// </summary>
    public class BattlePauseSituation : Situation {
        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);
            
            var situationService = Services.Resolve<SituationService>();
            var uiManager = Services.Resolve<UIManager>();
            var dialogUIService = uiManager.GetService<DialogUIService>();

            var itemLabels = new[] { "タイトルに戻る" };
            dialogUIService.OpenSelectionDialogAsync("ポーズメニュー", itemLabels, useBackgroundCancel: true, ct: scope.Token)
                .ContinueWith(result => {
                    switch (result) {
                        case 0:
                            situationService.Transition<TitleTopSituation>(transitionType: SituationService.TransitionType.SceneDefault);
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