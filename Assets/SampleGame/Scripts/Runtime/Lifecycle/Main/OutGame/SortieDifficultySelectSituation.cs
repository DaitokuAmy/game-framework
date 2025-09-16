using System.Collections.Generic;
using GameFramework.UISystems;
using SampleGame.Presentation.Battle;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 出撃画面用の難易度選択画面Situation
    /// </summary>
    public class SortieDifficultySelectSituation : ScreenSituation<SortieUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(SortieUIService service, List<UIScreen> screens) {
            screens.Add(service.DifficultySelectScreen);
        }
    }
}