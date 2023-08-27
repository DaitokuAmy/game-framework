using GameFramework.ModelSystems;
using UniRx;

namespace SampleGame.Field {
    /// <summary>
    /// プレイヤーの装備用モデル
    /// </summary>
    public class PlayerEquipmentModel : AutoIdModel<PlayerEquipmentModel> {
        private ReactiveProperty<UserWeaponModel> _weaponModel = new();
        private ReactiveProperty<UserArmorModel> _helmArmorModel = new();
        private ReactiveProperty<UserArmorModel> _bodyArmorModel = new();
        private ReactiveProperty<UserArmorModel> _armsArmorModel = new();
        private ReactiveProperty<UserArmorModel> _legsArmorModel = new();

        /// <summary>現在装備中の武器</summary>
        public IReadOnlyReactiveProperty<UserWeaponModel> WeaponModel => _weaponModel;
        /// <summary>現在装備中の頭防具</summary>
        public IReadOnlyReactiveProperty<UserArmorModel> HelmArmorModel => _helmArmorModel;
        /// <summary>現在装備中の体防具</summary>
        public IReadOnlyReactiveProperty<UserArmorModel> BodyArmorModel => _bodyArmorModel;
        /// <summary>現在装備中の腕防具</summary>
        public IReadOnlyReactiveProperty<UserArmorModel> ArmsArmorModel => _armsArmorModel;
        /// <summary>現在装備中の脚防具</summary>
        public IReadOnlyReactiveProperty<UserArmorModel> LegsArmorModel => _legsArmorModel;

        /// <summary>
        /// 武器の変更
        /// </summary>
        public void EquipWeapon(UserWeaponModel weaponModel) {
            _weaponModel.Value = weaponModel;
        }
        
        /// <summary>
        /// 防具の変更
        /// </summary>
        public void EquipArmor(UserArmorModel armorModel){
            switch (armorModel.ArmorType) {
                case ArmorType.Helm:
                    _helmArmorModel.Value = armorModel;
                    break;
                case ArmorType.Body:
                    _bodyArmorModel.Value = armorModel;
                    break;
                case ArmorType.Arms:
                    _armsArmorModel.Value = armorModel;
                    break;
                case ArmorType.Legs:
                    _legsArmorModel.Value = armorModel;
                    break;
            }
        }

        /// <summary>
        /// 防具の削除
        /// </summary>
        public void RemoveArmor(ArmorType armorType) {
            switch (armorType) {
                case ArmorType.Helm:
                    _helmArmorModel.Value = null;
                    break;
                case ArmorType.Body:
                    _bodyArmorModel.Value = null;
                    break;
                case ArmorType.Arms:
                    _armsArmorModel.Value = null;
                    break;
                case ArmorType.Legs:
                    _legsArmorModel.Value = null;
                    break;
            }
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private PlayerEquipmentModel(int id) : base(id) {
        }
    }
}
