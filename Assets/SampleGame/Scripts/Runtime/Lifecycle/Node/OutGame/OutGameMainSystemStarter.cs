using System;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class OutGameMainSystemStarter : MainSystemStarter {
        /// <inheritdoc/>
        protected override Type NavNodeType => typeof(SortieTopScreenNode);
    }
}