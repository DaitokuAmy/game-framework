using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Field {
    /// <summary>
    /// 装備画面のトップ用UIScreen
    /// </summary>
    public class FieldEquipmentTopUIScreen : UIScreen {
        [SerializeField, Tooltip("武器")]
        private WeaponUIView _weaponUIView;
        [SerializeField, Tooltip("頭防具")]
        private ArmorUIView _helmArmorUIView;
        [SerializeField, Tooltip("体防具")]
        private ArmorUIView _bodyArmorUIView;
        [SerializeField, Tooltip("腕防具")]
        private ArmorUIView _armsArmorUIView;
        [SerializeField, Tooltip("脚防具")]
        private ArmorUIView _legsArmorUIView;

        /// <summary>武器</summary>
        public WeaponUIView WeaponUIView => _weaponUIView;
        /// <summary>頭防具</summary>
        public ArmorUIView HelmArmorUIView => _helmArmorUIView;
        /// <summary>体防具</summary>
        public ArmorUIView BodyArmorUIView => _bodyArmorUIView;
        /// <summary>腕防具</summary>
        public ArmorUIView ArmsArmorUIView => _armsArmorUIView;
        /// <summary>脚防具</summary>
        public ArmorUIView LegsArmorUIView => _legsArmorUIView;
    }
}