using System.Collections.Generic;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Battle;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 出撃画面用の基礎Situation
    /// </summary>
    public class SortieSituation : ScreenSituation<SortieUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(SortieUIService service, List<UIScreen> screens) {
            screens.Add(service.BaseScreen);
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
        }
    }
}