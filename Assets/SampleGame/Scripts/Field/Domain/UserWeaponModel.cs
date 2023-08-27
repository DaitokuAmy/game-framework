using GameFramework.ModelSystems;

namespace SampleGame.Field {
    /// <summary>
    /// プレイヤーの武器用モデル
    /// </summary>
    public class UserWeaponModel : IdModel<int, UserWeaponModel> {
        /// <summary>物理攻撃力</summary>
        public int PhysicalAttack { get; private set; }
        /// <summary>魔法攻撃力</summary>
        public int MagicalAttack { get; private set; }
        /// <summary>属性</summary>
        public ElementType ElementType { get; private set; }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Setup(int physicalAttack, int magicalAttack, ElementType elementType) {
            PhysicalAttack = physicalAttack;
            MagicalAttack = magicalAttack;
            ElementType = elementType;
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UserWeaponModel(int id) : base(id) {
        }
    }
}
