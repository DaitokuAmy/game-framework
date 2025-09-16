using System.Collections.Generic;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Battle;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 出撃画面用のミッション選択画面Situation
    /// </summary>
    public class SortieMissionSelectSituation : ScreenSituation<SortieUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(SortieUIService service, List<UIScreen> screens) {
            screens.Add(service.MissionSelectScreen);
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
        }
    }
}