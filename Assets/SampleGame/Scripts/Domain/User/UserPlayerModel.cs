using GameFramework.Core;
using GameFramework.ModelSystems;
using SampleGame.Domain.Common;
using SampleGame.Domain.Equipment;
using UniRx;

namespace SampleGame.Domain.User {
    /// <summary>
    /// UserPlayerModelの読み取り用インターフェース
    /// </summary>
    public interface IReadOnlyUserPlayerModel {
        /// <summary>プレイヤー参照用</summary>
        IReadOnlyPlayerModel PlayerModel { get; }
        
        /// <summary>武器モデル参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyWeaponModel> WeaponModel { get; }
        /// <summary>頭防具モデル参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyArmorModel> HelmArmorModel { get; }
        /// <summary>体防具モデル参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyArmorModel> BodyArmorModel { get; }
        /// <summary>腕防具モデル参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyArmorModel> ArmsArmorModel { get; }
        /// <summary>脚防具モデル参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyArmorModel> LegsArmorModel { get; }
    }
    
    /// <summary>
    /// ユーザー操作対象のプレイヤーキャラモデル
    /// </summary>
    public class UserPlayerModel : AutoIdModel<UserPlayerModel>, IReadOnlyUserPlayerModel {
        private PlayerModel _playerModel;
        private ReactiveProperty<WeaponModel> _weaponModel;
        private ReactiveProperty<ArmorModel> _helmArmorModel;
        private ReactiveProperty<ArmorModel> _bodyArmorModel;
        private ReactiveProperty<ArmorModel> _armsArmorModel;
        private ReactiveProperty<ArmorModel> _legsArmorModel;

        /// <summary>プレイヤー参照用</summary>
        IReadOnlyPlayerModel IReadOnlyUserPlayerModel.PlayerModel => _playerModel;
        /// <summary>武器参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyWeaponModel> IReadOnlyUserPlayerModel.WeaponModel => _weaponModel.Select(x => (IReadOnlyWeaponModel)x).ToReactiveProperty();
        /// <summary>頭防具参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyArmorModel> IReadOnlyUserPlayerModel.HelmArmorModel => _helmArmorModel.Select(x => (IReadOnlyArmorModel)x).ToReactiveProperty();
        /// <summary>体防具参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyArmorModel> IReadOnlyUserPlayerModel.BodyArmorModel => _bodyArmorModel.Select(x => (IReadOnlyArmorModel)x).ToReactiveProperty();
        /// <summary>腕防具参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyArmorModel> IReadOnlyUserPlayerModel.ArmsArmorModel => _armsArmorModel.Select(x => (IReadOnlyArmorModel)x).ToReactiveProperty();
        /// <summary>脚防具参照用</summary>
        IReadOnlyReactiveProperty<IReadOnlyArmorModel> IReadOnlyUserPlayerModel.LegsArmorModel => _legsArmorModel.Select(x => (IReadOnlyArmorModel)x).ToReactiveProperty();

        /// <summary>プレイヤー参照用</summary>
        public PlayerModel PlayerModel => _playerModel;
        /// <summary>武器参照用</summary>
        public WeaponModel WeaponModel => _weaponModel.Value;
        /// <summary>頭防具参照用</summary>
        public ArmorModel HelmArmorModel => _helmArmorModel.Value;
        /// <summary>体防具参照用</summary>
        public ArmorModel BodyArmorModel => _bodyArmorModel.Value;
        /// <summary>腕防具参照用</summary>
        public ArmorModel ArmsArmorModel => _armsArmorModel.Value;
        /// <summary>脚防具参照用</summary>
        public ArmorModel LegsArmorModel => _legsArmorModel.Value;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected UserPlayerModel(int id) : base(id) {
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);

            _weaponModel = new ReactiveProperty<WeaponModel>().ScopeTo(scope);
            _helmArmorModel = new ReactiveProperty<ArmorModel>().ScopeTo(scope);
            _bodyArmorModel = new ReactiveProperty<ArmorModel>().ScopeTo(scope);
            _armsArmorModel = new ReactiveProperty<ArmorModel>().ScopeTo(scope);
            _legsArmorModel = new ReactiveProperty<ArmorModel>().ScopeTo(scope);
        }

        /// <summary>
        /// Player情報の設定
        /// </summary>
        public void SetPlayer(PlayerModel playerModel) {
            _playerModel = playerModel;
        }

        /// <summary>
        /// 武器の設定
        /// </summary>
        public void SetWeapon(WeaponModel weaponModel) {
            _weaponModel.Value = weaponModel;
        }
        
        /// <summary>
        /// 防具の設定
        /// </summary>
        public void SetArmor(ArmorModel armorModel) {
            switch (armorModel.MasterData.ArmorType) {
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
    }
}
