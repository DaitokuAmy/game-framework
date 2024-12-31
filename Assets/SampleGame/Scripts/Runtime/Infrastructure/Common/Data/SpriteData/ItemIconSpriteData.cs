using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// アイテムアイコン用スプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_item_icon_0001.asset", menuName = "SampleGame/Sprite Data/Item Icon")]
    public class ItemIconSpriteData : ScriptableObject {
        [Tooltip("スプライト")]
        public Sprite sprite;
    }
}