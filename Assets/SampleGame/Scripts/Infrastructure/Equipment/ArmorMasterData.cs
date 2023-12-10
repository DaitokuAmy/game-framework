using SampleGame.Domain.Common;
using SampleGame.Domain.Equipment;

namespace Opera.Infrastructure.Equipment {
    /// <summary>
    /// ArmorMasterData
    /// </summary>
    public class ArmorMasterData : IArmorMasterData {
        public string name;
        public string prefabAssetKey;
        public int physicalDefense;
        public int magicalDefense;
        public ArmorType armorType;

        /// <inheritdoc/>
        string IEquipmentMasterData.Name => name;
        /// <inheritdoc/>
        string IEquipmentMasterData.PrefabAssetKey => prefabAssetKey;
        /// <inheritdoc/>
        int IArmorMasterData.PhysicalDefense => physicalDefense;
        /// <inheritdoc/>
        int IArmorMasterData.MagicalDefense => magicalDefense;
        /// <inheritdoc/>
        ArmorType IArmorMasterData.ArmorType => armorType;
    }
}