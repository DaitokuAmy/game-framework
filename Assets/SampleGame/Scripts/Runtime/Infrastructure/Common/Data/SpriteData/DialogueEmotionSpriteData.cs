using System;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// エモーションスプライトデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_sprite_dialogue_emotion.asset", menuName = "SampleGame/Sprite Data/Dialogue Emotion Icon")]
    public class DialogueEmotionSpriteData : ScriptableObject {
        /// <summary>
        /// アイコン情報
        /// </summary>
        [Serializable]
        public class IconInfo {
            [Tooltip("アイコンキー")]
            public string key;
            [Tooltip("アイコン")]
            public GameObject spritePrefab;
        }

        [Tooltip("エモーション用のアイコン")]
        public IconInfo[] iconInfos;
    }
}