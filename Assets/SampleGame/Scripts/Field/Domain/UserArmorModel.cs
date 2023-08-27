using GameFramework.ModelSystems;

namespace SampleGame.Field {
    /// <summary>
    /// プレイヤーの防具用モデル
    /// </summary>
    public class UserArmorModel : IdModel<int, UserArmorModel> {
        /// <summary>防具タイプ</summary>
        public ArmorType ArmorType { get; private set; }
        /// <summary>物理防御力</summary>
        public int PhysicalDefense { get; private set; }
        /// <summary>魔法防御力</summary>
        public int MagicalDefense { get; private set; }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Setup(ArmorType armorType, int physicalDefense, int magicalDefense) {
            ArmorType = armorType;
            PhysicalDefense = physicalDefense;
            MagicalDefense = magicalDefense;
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UserArmorModel(int id) : base(id) {
        }
    }
}
