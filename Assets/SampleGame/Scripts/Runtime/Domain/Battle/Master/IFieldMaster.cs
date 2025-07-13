namespace SampleGame.Domain.Battle {
    /// <summary>
    /// フィールドマスター用インターフェース
    /// </summary>
    public interface IFieldMaster {
        /// <summary>識別子</summary>
        int Id { get; }
        /// <summary>名称</summary>
        string Name { get; }
        /// <summary>アセットキー</summary>
        string AssetKey { get; }
    }
}