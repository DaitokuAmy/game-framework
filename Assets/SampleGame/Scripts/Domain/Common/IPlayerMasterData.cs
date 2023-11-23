namespace SampleGame.Domain.Common {
    /// <summary>
    /// プレイヤーマスターデータ用インターフェース
    /// </summary>
    public interface IPlayerMasterData {
        /// <summary>名前</summary>
        string Name { get; }
        /// <summary>Prefab読み込み用のアセットキー</summary>
        string PrefabAssetKey { get; }
    }
}