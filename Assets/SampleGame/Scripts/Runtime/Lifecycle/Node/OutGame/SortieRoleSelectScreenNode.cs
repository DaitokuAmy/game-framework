using System.Collections.Generic;
using GameFramework.UISystems;
using SampleGame.Presentation.OutGame;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// 出撃画面用の兵科選択画面のScreenNode
    /// </summary>
    public class SortieRoleSelectScreenNode : ScreenNode<SortieUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(SortieUIService service, List<UIScreen> screens) {
            screens.Add(service.RoleSelectScreen);
        }
    }
}