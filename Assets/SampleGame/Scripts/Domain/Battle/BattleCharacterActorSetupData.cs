using System;
using UnityEngine;
using SampleGame.Domain.Common;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用キャラアクター初期化用データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_battle_character_actor_setup_pl000.asset", menuName = "SampleGame/Battle/Character Actor Setup Data")]
    public class BattleCharacterActorSetupData : ScriptableObject {
        /// <summary>
        /// 移動アクションの情報
        /// </summary>
        [Serializable]
        public class MoveActionInfo {
            [Tooltip("移動速度")]
            public float speed = 1.0f;
            [Tooltip("加速度")]
            public float acceleration = 10.0f;
            [Tooltip("減速度")]
            public float brake = 1.0f;
            [Tooltip("角速度")]
            public float angularSpeed = 720.0f;
        }

        /// <summary>
        /// ジャンプアクションの情報
        /// </summary>
        [Serializable]
        public class JumpActionInfo {
            [Tooltip("ジャンプ用のアクション")]
            public TriggerStateActorAction action;
        }
        
        /// <summary>
        /// ダメージアクションの情報
        /// </summary>
        [Serializable]
        public class DamageActionInfo {
            [Tooltip("ノックバック用のアクション")]
            public TriggerStateActorAction knockBackAction;
            [Tooltip("加算ダメージ用クリップ")]
            public AnimationClip vibrateClip;
        }

        /// <summary>
        /// スキルアクションの情報
        /// </summary>
        [Serializable]
        public class SkillActionInfo {
            [Tooltip("スキル検索用のキー")]
            public string key = "Unknown";
            [Tooltip("スキルアクション")]
            public GeneralActorAction action;
        }
        
        [Header("基本情報")]
        [Tooltip("基礎となるAnimatorController")]
        public RuntimeAnimatorController controller;
        [Tooltip("ユニークカメラ登録用のGroupPrefab")]
        public GameObject cameraGroupPrefab;

        [Header("アクション情報")]
        [Tooltip("移動アクション情報")]
        public MoveActionInfo moveActionInfo;
        [Tooltip("ジャンプアクション情報")]
        public JumpActionInfo jumpActionInfo;
        [Tooltip("ダメージアクション情報")]
        public DamageActionInfo damageActionInfo;
        [Tooltip("スキルアクション情報リスト")]
        public SkillActionInfo[] skillActionInfos;
        
        [Header("ブレンド情報")]
        [Tooltip("エイム時の回転ダンピング")]
        public float aimDamping = 0.2f;
        
        [Header("テストデータ")]
        [Tooltip("テスト用の弾Prefab")]
        public GameObject bulletPrefab;
        [Tooltip("テスト用のオーラPrefab")]
        public GameObject auraPrefab;
    }
}