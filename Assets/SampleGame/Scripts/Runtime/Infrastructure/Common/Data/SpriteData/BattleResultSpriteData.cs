using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// バトルリザルト用のデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_battle_result_ch000_00.asset", menuName = "SampleGame/Sprite Data/Battle Result")]
    public class BattleResultSpriteData : ScriptableObject {
        [Tooltip("キャラクタースプライト")]
        public Sprite characterSprite;
        [Tooltip("背景スプライト")]
        public Sprite bgSprite;
    }
}