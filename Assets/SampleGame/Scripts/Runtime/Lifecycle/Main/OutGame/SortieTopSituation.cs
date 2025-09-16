using System.Collections.Generic;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Battle;
using R3;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 出撃画面用のトップ画面Situation
    /// </summary>
    public class SortieTopSituation : ScreenSituation<SortieUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(SortieUIService service, List<UIScreen> screens) {
            screens.Add(service.TopScreen);
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
            UIService.TopScreen.SelectedIndexSubject
                .TakeUntil(scope)
                .Subscribe(index => {
                    switch (index) {
                        case 0:
                            ChangeSituation<SortieMissionSelectSituation>();
                            break;
                        default:
                            ChangeSituation<SortieRoleSelectSituation>();
                            break;
                    }
                });
        }
    }
}