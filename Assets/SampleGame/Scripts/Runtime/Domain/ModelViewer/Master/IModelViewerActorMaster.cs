namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// ModelViewer用のマスターインタフェース
    /// </summary>
    public interface IModelViewerActorMaster {
        /// <summary>識別子</summary>
        int Id { get; }
        /// <summary>表示名</summary>
        string Name { get; }
        /// <summary>アセットキー</summary>
        string AssetKey { get; }
    }
}