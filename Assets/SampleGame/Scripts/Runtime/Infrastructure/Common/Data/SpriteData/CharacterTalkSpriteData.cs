using System;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// 会話用スプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_talk_ch000_00.asset", menuName = "SampleGame/Sprite Data/Character Talk")]
    public class CharacterTalkSpriteData : ScriptableObject {
        [Serializable]
        public class SpriteInfo {
            public string facialId = "";
            public Sprite sprite;
            // public Sprite bgSprite;
            public string talkBgId;
        }
        
        [Tooltip("スプライト情報")]
        public SpriteInfo[] spriteInfos;
    }
}