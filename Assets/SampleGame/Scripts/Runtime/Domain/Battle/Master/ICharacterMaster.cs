namespace SampleGame.Domain.Battle {
    /// <summary>
    /// キャラ用のマスター
    /// </summary>
    public interface ICharacterMaster {
        /// <summary>識別子</summary>
        int Id { get; }
        /// <summary>名称</summary>
        string Name { get; }
        /// <summary>アセットキー</summary>
        string AssetKey { get; }
        /// <summary>アクター制御用アセットキー</summary>
        string ActorAssetKey { get; }
    }
}
