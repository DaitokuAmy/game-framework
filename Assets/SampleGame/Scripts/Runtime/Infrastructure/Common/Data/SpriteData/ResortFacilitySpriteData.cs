using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// リゾートの施設用のスプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_resort_facility_at001_00.asset", menuName = "SampleGame/Sprite Data/Resort/Facility")]
    public class ResortFacilitySpriteData : ScriptableObject {
        [Tooltip("スプライト")]
        public Sprite sprite;
    }
}