using GameFramework.Core;
using SampleGame.Domain.Equipment;

namespace SampleGame.Application.Equipment {
    /// <summary>
    /// マスターデータ読み込み用リポジトリ
    /// </summary>
    public interface IEquipmentMasterRepository {
        /// <summary>
        /// Weaponのマスターデータ読み込み
        /// </summary>
        IProcess<IWeaponMaster> LoadWeapon(int id);
        
        /// <summary>
        /// Armorのマスターデータ読み込み
        /// </summary>
        IProcess<IArmorMaster> LoadArmor(int id);
    }
}
