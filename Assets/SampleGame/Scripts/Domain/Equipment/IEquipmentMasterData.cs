namespace SampleGame.Domain.Equipment {
    /// <summary>
    /// 装備用データ
    /// </summary>
    public interface IEquipmentMasterData {
        /// <summary>表示名</summary>
        string Name { get; }
        /// <summary>Prefab読み込み用のキー</summary>
        string PrefabAssetKey { get; }
    }
}