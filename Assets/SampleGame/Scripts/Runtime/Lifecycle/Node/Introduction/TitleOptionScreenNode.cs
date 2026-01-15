using System.Collections.Generic;
using GameFramework.UISystems;
using SampleGame.Presentation.Introduction;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// TitleOption用のScreenNode
    /// </summary>
    public class TitleOptionScreenNode : ScreenNode<IntroductionUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(IntroductionUIService service, List<UIScreen> screens) {
            screens.Add(service.TitleOptionUIScreen);
        }
    }
}