namespace SampleGame.Domain.Equipment {
    /// <summary>
    /// 武器用の読み取り専用モデル
    /// </summary>
    public interface IReadOnlyWeaponModel : IReadOnlyEquipmentModel {
    }
    
    /// <summary>
    /// 武器のドメインモデル
    /// </summary>
    public class WeaponModel : EquipmentModel, IReadOnlyWeaponModel {
        /// <summary>マスターデータ</summary>
        public IWeaponMaster MasterData { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected WeaponModel(int id) : base(id) {
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="masterData">マスターデータ</param>
        public void Setup(IWeaponMaster masterData) {
            MasterData = masterData;
        }
    }
}