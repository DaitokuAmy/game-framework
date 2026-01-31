using System.Collections.Generic;
using GameFramework.UISystem;
using SampleGame.Presentation.OutGame;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 出撃画面用のミッション選択画面のScreenNode
    /// </summary>
    public class SortieMissionSelectScreenNode : ScreenNode<SortieUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(SortieUIService service, List<UIScreen> screens) {
            screens.Add(service.MissionSelectScreen);
        }
    }
}