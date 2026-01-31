using System.Collections.Generic;
using GameFramework.UISystem;
using SampleGame.Presentation.Introduction;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// TitleTop用のScreenNode
    /// </summary>
    public class TitleTopScreenNode : ScreenNode<IntroductionUIService> {
        /// <inheritdoc/>
        protected override void GetScreens(IntroductionUIService service, List<UIScreen> screens) {
            screens.Add(service.TitleTopUIScreen);
        }
    }
}