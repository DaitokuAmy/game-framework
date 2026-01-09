using System.Collections.Generic;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Battle;
using R3;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle中HudのScreenNode
    /// </summary>
    public class BattleHudScreenNode : ScreenNode<BattleHudUIService> {
        /// <inheritdoc/>
        protected override void Activate(TransitionHandle<INavNode> handle, IScope scope) {
            base.Activate(handle, scope);

            UIService.BattleHudUIScreen.ClickedMenuButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    AppNavigator.TransitionToBattlePause();
                });
        }

        /// <inheritdoc/>
        protected override void GetScreens(BattleHudUIService service, List<UIScreen> screens) {
            screens.Add(service.BattleHudUIScreen);
        }
    }
}