using GameFramework.Core;
using SampleGame.Domain.Equipment;

namespace SampleGame.Domain.Common {
    /// <summary>
    /// マスターデータ読み込み用リポジトリ
    /// </summary>
    public interface IEquipmentMasterDataRepository {
        /// <summary>
        /// Weaponのマスターデータ読み込み
        /// </summary>
        IProcess<IWeaponMasterData> LoadWeapon(int id);
        
        /// <summary>
        /// Armorのマスターデータ読み込み
        /// </summary>
        IProcess<IArmorMasterData> LoadArmor(int id);
    }
}
