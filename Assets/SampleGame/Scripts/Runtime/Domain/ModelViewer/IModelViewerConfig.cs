namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// モデルビューア設定の参照インターフェース
    /// </summary>
    public interface IModelViewerConfig {
        /// <summary>カメラ設定</summary>
        IPreviewCameraMaster Camera { get; }
    }
}
