using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Field {
    /// <summary>
    /// 装備画面の武器リスト用UIScreen
    /// </summary>
    public class FieldEquipmentWeaponListUIScreen : UIScreen {
        [SerializeField, Tooltip("武器リスト用のUIViewテンプレート")]
        private WeaponUIView _weaponUIViewTemplate;
        [SerializeField, Tooltip("武器詳細情報")]
        private WeaponDetailUIView _weaponDetailUIView;
    }
}