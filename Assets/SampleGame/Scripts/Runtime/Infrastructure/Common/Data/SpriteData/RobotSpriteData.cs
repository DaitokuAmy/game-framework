using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// ロボット用のスプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_rb000_00.asset", menuName = "SampleGame/Sprite Data/Robot")]
    public class RobotSpriteData : ScriptableObject {
        [Tooltip("バストアップ用スプライト")]
        public Sprite bustUpSprite;
        [Tooltip("腰上用スプライト")]
        public Sprite waistUpSprite;
        [Tooltip("全身用スプライト")]
        public Sprite fullSprite;
    }
}