using SampleGame.Domain.Common;
using SampleGame.Domain.Equipment;

namespace Opera.Infrastructure.Equipment {
    /// <summary>
    /// WeaponMasterData
    /// </summary>
    public class WeaponMasterData : IWeaponMasterData {
        public string name;
        public string prefabAssetKey;
        public int physicalAttack;
        public int magicalAttack;
        public ElementType elementType;
        
        /// <inheritdoc/>
        string IEquipmentMasterData.Name => name;
        /// <inheritdoc/>
        string IEquipmentMasterData.PrefabAssetKey => prefabAssetKey;
        /// <inheritdoc/>
        int IWeaponMasterData.PhysicalAttack => physicalAttack;
        /// <inheritdoc/>
        int IWeaponMasterData.MagicalAttack => magicalAttack;
        /// <inheritdoc/>
        ElementType IWeaponMasterData.ElementType => elementType;
    }
}