using System;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class UITestMainSystemStarter : MainSystemStarter {
        /// <inheritdoc/>
        protected override Type SituationType => typeof(UITestSceneSituation);
    }
}