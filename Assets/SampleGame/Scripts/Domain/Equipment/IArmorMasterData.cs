using SampleGame.Domain.Common;

namespace SampleGame.Domain.Equipment {
    /// <summary>
    /// 防具用データ
    /// </summary>
    public interface IArmorMasterData : IEquipmentMasterData {
        /// <summary>物理防御力</summary>
        int PhysicalDefense { get; }
        /// <summary>魔法防御力</summary>
        int MagicalDefense { get; }
        /// <summary>防具タイプ</summary>
        ArmorType ArmorType { get; }
    }
}