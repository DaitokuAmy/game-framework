using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// リゾートの客要求用のスプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_resort_customer_needs_drink.asset", menuName = "SampleGame/Sprite Data/Resort/Customer Needs")]
    public class ResortCustomerNeedsSpriteData : ScriptableObject {
        [Tooltip("スプライト")]
        public Sprite sprite;
    }
}