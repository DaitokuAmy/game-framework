using SampleGame.Domain.Common;

namespace SampleGame.Domain.Equipment {
    /// <summary>
    /// 武器用マスター
    /// </summary>
    public interface IWeaponMaster : IEquipmentMaster {
        /// <summary>物理攻撃力</summary>
        int PhysicalAttack { get; }
        /// <summary>魔法攻撃力</summary>
        int MagicalAttack { get; }
        /// <summary>属性</summary>
        ElementType ElementType { get; }
    }
}