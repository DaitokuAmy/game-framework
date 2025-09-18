namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// ModelViewer用の背景マスターインターフェース
    /// </summary>
    public interface IModelViewerEnvironmentMaster {
        /// <summary>表示名</summary>
        string Name { get; }
        /// <summary>背景読み込み用のアセットキー</summary>
        string AssetKey { get; }
    }
}