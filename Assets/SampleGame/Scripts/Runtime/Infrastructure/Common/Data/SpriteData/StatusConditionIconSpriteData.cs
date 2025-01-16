using System;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// 状態変化アイコンスプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_status_condition_icon.asset", menuName = "SampleGame/Sprite Data/Status Change Icon")]
    public class StatusConditionIconSpriteData : ScriptableObject {
        /// <summary>
        /// スプライト情報
        /// </summary>
        [Serializable]
        public class SpriteInfo {
            [Tooltip("メインアイコン")]
            public Sprite mainSprite;
            [Tooltip("サブアイコン")]
            public Sprite subSprite;
        }
        
        [Header("状態効果")]
        [Tooltip("攻撃力アップ")]
        public SpriteInfo attackUp;
        [Tooltip("攻撃力ダウン")]
        public SpriteInfo attackDown;
        [Tooltip("防御力アップ")]
        public SpriteInfo defenseUp;
        [Tooltip("防御力ダウン")]
        public SpriteInfo defenseDown;
        [Tooltip("クリティカル発生率アップ")]
        public SpriteInfo criticalProbabilityUp;
        [Tooltip("クリティカル発生率ダウン")]
        public SpriteInfo criticalProbabilityDown;
        [Tooltip("クリティカルダメージ倍率アップ")]
        public SpriteInfo criticalDamageRateUp;
        [Tooltip("クリティカルダメージ倍率ダウン")]
        public SpriteInfo criticalDamageRateDown;
        
        [Header("状態異常")]
        [Tooltip("燃焼")]
        public SpriteInfo burn;
        [Tooltip("麻痺")]
        public SpriteInfo paralyze;
    }
}