using SampleGame.Domain.Common;
using SampleGame.Domain.Equipment;

namespace Opera.Infrastructure.Equipment {
    /// <summary>
    /// ArmorMasterData
    /// </summary>
    public class ArmorMasterData : IArmorMaster {
        public string name;
        public string prefabAssetKey;
        public int physicalDefense;
        public int magicalDefense;
        public ArmorType armorType;

        /// <inheritdoc/>
        string IEquipmentMaster.Name => name;
        /// <inheritdoc/>
        string IEquipmentMaster.PrefabAssetKey => prefabAssetKey;
        /// <inheritdoc/>
        int IArmorMaster.PhysicalDefense => physicalDefense;
        /// <inheritdoc/>
        int IArmorMaster.MagicalDefense => magicalDefense;
        /// <inheritdoc/>
        ArmorType IArmorMaster.ArmorType => armorType;
    }
}