using System;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 戦闘用キャラアクター初期化データ
    /// </summary>
    [CreateAssetMenu(menuName = "Third Person Engine/Actor Data/Battle Character", fileName = "dat_battle_character_actor_setup_ch000_00.asset")]
    public class BattleCharacterActorData : CharacterActorSetupData {
        /// <summary>
        /// ジャンプアクション情報
        /// </summary>
        [Serializable]
        public class JumpActionInfo {
            [Tooltip("アクション")]
            public TriggerStateActorAction action;
            [Tooltip("初速")]
            public float initVelocity = 10.0f;
        }

        [Tooltip("地上状態とする高さ")]
        public float groundHeight = 0.1f;
        [Tooltip("空中状態とする高さ")]
        public float airHeight = 0.2f;
        [Tooltip("ジャンプアクション情報")]
        public JumpActionInfo jumpAction;
    }
}