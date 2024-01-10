namespace SampleGame.Domain.Equipment {
    /// <summary>
    /// 装備用マスター
    /// </summary>
    public interface IEquipmentMaster {
        /// <summary>表示名</summary>
        string Name { get; }
        /// <summary>Prefab読み込み用のキー</summary>
        string PrefabAssetKey { get; }
    }
}