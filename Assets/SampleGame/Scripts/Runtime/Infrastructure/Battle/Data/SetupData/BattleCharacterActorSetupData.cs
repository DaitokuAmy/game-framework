using System;
using SampleGame.Infrastructure;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// 戦闘用キャラアクター初期化データ
    /// </summary>
    [CreateAssetMenu(menuName = "SampleGame/Actor Setup/Battle Character", fileName = "dat_battle_character_actor_setup_ch000_00.asset")]
    public class BattleCharacterActorSetupData : CharacterActorSetupData {
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

        [Tooltip("ジャンプアクション情報")]
        public JumpActionInfo JumpAction;
    }
}