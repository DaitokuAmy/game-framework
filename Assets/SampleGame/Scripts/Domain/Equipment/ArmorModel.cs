namespace SampleGame.Domain.Equipment {
    /// <summary>
    /// 防具用の読み取り専用モデル
    /// </summary>
    public interface IReadOnlyArmorModel : IReadOnlyEquipmentModel {
    }
    
    /// <summary>
    /// 防具のドメインモデル
    /// </summary>
    public class ArmorModel : EquipmentModel, IReadOnlyArmorModel {
        /// <summary>マスターデータ</summary>
        public IArmorMasterData MasterData { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected ArmorModel(int id) : base(id) {
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="masterData">マスターデータ</param>
        public void Setup(IArmorMasterData masterData) {
            MasterData = masterData;
        }
    }
}