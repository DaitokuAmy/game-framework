namespace SampleGame.Domain.Common {
    /// <summary>
    /// プレイヤーマスター用インターフェース
    /// </summary>
    public interface IPlayerMaster {
        /// <summary>名前</summary>
        string Name { get; }
        /// <summary>Prefab読み込み用のアセットキー</summary>
        string PrefabAssetKey { get; }
    }
}