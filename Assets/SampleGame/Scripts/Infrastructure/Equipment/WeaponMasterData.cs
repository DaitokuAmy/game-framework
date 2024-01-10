using SampleGame.Domain.Common;
using SampleGame.Domain.Equipment;

namespace Opera.Infrastructure.Equipment {
    /// <summary>
    /// WeaponMasterData
    /// </summary>
    public class WeaponMasterData : IWeaponMaster {
        public string name;
        public string prefabAssetKey;
        public int physicalAttack;
        public int magicalAttack;
        public ElementType elementType;
        
        /// <inheritdoc/>
        string IEquipmentMaster.Name => name;
        /// <inheritdoc/>
        string IEquipmentMaster.PrefabAssetKey => prefabAssetKey;
        /// <inheritdoc/>
        int IWeaponMaster.PhysicalAttack => physicalAttack;
        /// <inheritdoc/>
        int IWeaponMaster.MagicalAttack => magicalAttack;
        /// <inheritdoc/>
        ElementType IWeaponMaster.ElementType => elementType;
    }
}