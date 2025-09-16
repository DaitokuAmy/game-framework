using System.Collections.Generic;
using GameFramework.UISystems;
using SampleGame.Presentation.Battle;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 出撃画面用のトップ画面Situation
    /// </summary>
    public class SortieTopSituation : ScreenSituation<SortieUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(SortieUIService service, List<UIScreen> screens) {
            screens.Add(service.TopScreen);
        }
    }
}