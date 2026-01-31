using System;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class IntroductionMainSystemStarter : MainSystemStarter {
        /// <inheritdoc/>
        protected override int NodeId => AppNavigator.Id.TitleTop;
    }
}