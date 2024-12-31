using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// キャラクターアイコン用スプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_character_icon_ch000_00.asset", menuName = "SampleGame/Sprite Data/Character Icon")]
    public class CharacterIconSpriteData : ScriptableObject {
        [Tooltip("スプライト")]
        public Sprite sprite;
    }
}