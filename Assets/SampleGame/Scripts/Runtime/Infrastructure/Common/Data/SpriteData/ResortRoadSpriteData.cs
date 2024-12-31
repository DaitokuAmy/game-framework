using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// リゾートの道用のスプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_resort_road_001.asset", menuName = "SampleGame/Sprite Data/Resort/Road")]
    public class ResortRoadSpriteData : ScriptableObject {
        [Tooltip("スプライト")]
        public Sprite sprite;
    }
}