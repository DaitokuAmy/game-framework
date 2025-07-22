using System;
using TMPro;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// キャラアクター初期化データ
    /// </summary>
    public abstract class CharacterActorData : ScriptableObject {
        /// <summary>
        /// アクション情報
        /// </summary>
        [Serializable]
        public class ActionInfo {
            public string actionKey = "";
            public GeneralActorAction action;
        }

        /// <summary>
        /// 移動アクション情報
        /// </summary>
        [Serializable]
        public class MoveActionInfo : IMoveResolverContext, VelocityActorComponent.ISettings {
            [Tooltip("最大速度")]
            public float maxSpeed;
            [Tooltip("加速度")]
            public float acceleration;
            [Tooltip("ブレーキ")]
            public float brake;
            [Tooltip("ブレーキ(空中)")]
            public float airBrake;
            [Tooltip("角速度")]
            public float angularSpeed;
            [Tooltip("移動誤差")]
            public float moveThreshold = 0.05f;
            [Tooltip("重力")]
            public float gravity = -9.81f;

            public float MaxSpeed => maxSpeed;
            public float Acceleration => acceleration;
            public float Brake => brake;
            public float AngularSpeed => angularSpeed;
            public float AirBrake => airBrake;
            public float GroundBrake => brake;
            public float Gravity => gravity;
        }
        
        [Tooltip("基本モーション用のアニメーターコントローラ")]
        public RuntimeAnimatorController baseController;
        [Tooltip("アクション情報")]
        public ActionInfo[] actionInfos;
        [Tooltip("移動アクション情報")]
        public MoveActionInfo moveActionInfo;
    }
}