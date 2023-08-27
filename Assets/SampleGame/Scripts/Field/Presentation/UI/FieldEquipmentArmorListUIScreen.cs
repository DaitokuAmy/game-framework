using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Field {
    /// <summary>
    /// 装備画面の防具リスト用UIScreen
    /// </summary>
    public class FieldEquipmentArmorListUIScreen : UIScreen {
        [SerializeField, Tooltip("防具リスト用のUIViewテンプレート")]
        private ArmorUIView _armorUIViewTemplate;
        [SerializeField, Tooltip("防具詳細情報")]
        private ArmorDetailUIView _armorDetailUIView;
    }
}