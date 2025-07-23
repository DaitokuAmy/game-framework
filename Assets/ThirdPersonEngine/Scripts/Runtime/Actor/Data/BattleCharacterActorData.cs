using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPersonEngine {
    /// <summary>
    /// 戦闘用キャラアクター初期化データ
    /// </summary>
    [CreateAssetMenu(menuName = "Third Person Engine/Actor Data/Battle Character", fileName = "dat_battle_character_actor_setup_ch000_00.asset")]
    public class BattleCharacterActorData : CharacterActorData {
        /// <summary>
        /// ジャンプアクション情報
        /// </summary>
        [Serializable]
        public class JumpActionInfo {
            [Tooltip("立ち中アクション")]
            public AnimationClipActorAction standingAction;
            [Tooltip("移動中アクション")]
            public AnimationClipActorAction movingAction;
            [Tooltip("移動速度係数")]
            public float moveSpeedScale = 0.5f;
        }

        /// <summary>
        /// センサー情報
        /// </summary>
        [Serializable]
        public class SensorInfo : SensorActorComponent.ISettings {
            [Tooltip("地面センサーの半径")]
            public float groundSensorRadius = 0.5f;
            [Tooltip("地面センサーの中心位置オフセット")]
            public float groundSensorOffsetY = 0.5f;

            /// <inheritdoc/>
            float SensorActorComponent.ISettings.GroundSensorRadius => groundSensorRadius;
            /// <inheritdoc/>
            float SensorActorComponent.ISettings.GroundSensorOffsetY => groundSensorOffsetY;
        }

        [Tooltip("ジャンプアクション情報")]
        public JumpActionInfo jumpActionInfo;
        [Tooltip("センサー情報")]
        public SensorInfo sensorInfo;
    }
}