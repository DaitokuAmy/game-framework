using System;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// ModelViewerを直接開始するためのStarter
    /// </summary>
    public class ModelViewerMainSystemStarter : MainSystemStarter {
        /// <inheritdoc/>
        protected override Type NavNodeType => typeof(ModelViewerSceneSessionNode);
    }
}