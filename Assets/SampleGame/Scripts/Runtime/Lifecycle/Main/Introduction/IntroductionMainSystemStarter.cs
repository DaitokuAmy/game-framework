using System;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class IntroductionMainSystemStarter : MainSystemStarter {
        /// <inheritdoc/>
        protected override Type SituationType => typeof(TitleTopSituation);
    }
}