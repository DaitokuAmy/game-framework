using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// キャラサムネ用のデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_character_ch000_00.asset", menuName = "SampleGame/Sprite Data/Character")]
    public class CharacterSpriteData : ScriptableObject {
        [Tooltip("顔用スプライト")]
        public Sprite faceSprite;
        [Tooltip("バストアップ用スプライト")]
        public Sprite bustUpSprite;
        [Tooltip("全体用スプライト")]
        public Sprite fullSprite;
    }
}