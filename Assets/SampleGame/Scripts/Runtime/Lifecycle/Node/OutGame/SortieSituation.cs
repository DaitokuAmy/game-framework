using System.Collections.Generic;
using GameFramework.UISystem;
using SampleGame.Presentation.OutGame;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 出撃画面用の基礎ScreenNode
    /// </summary>
    public class SortieScreenNode : ScreenNode<SortieUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(SortieUIService service, List<UIScreen> screens) {
            screens.Add(service.BaseScreen);
        }
    }
}