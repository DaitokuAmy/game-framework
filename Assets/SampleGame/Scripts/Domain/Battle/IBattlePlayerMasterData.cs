namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用プレイヤーマスターデータ用インターフェース
    /// </summary>
    public interface IBattlePlayerMasterData {
        /// <summary>アクター制御用データアセットキー</summary>
        string ActorSetupDataAssetKey { get; }
        /// <summary>最大体力</summary>
        int HealthMax { get; }
    }
}