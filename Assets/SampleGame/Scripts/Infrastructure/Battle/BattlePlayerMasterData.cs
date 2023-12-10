using System.Collections.Generic;
using SampleGame.Domain.Battle;
using UnityEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトル用のプレイヤーマスター
    /// </summary>
    [CreateAssetMenu(fileName = "dat_master_battle_player_pl000.asset", menuName = "SampleGame/Master Data/Battle Player")]
    public class BattlePlayerMasterData : ScriptableObject, IBattlePlayerMaster {
        [Tooltip("体力の最大値")]
        public int healthMax = 100;
        [Tooltip("アクター制御用SetupDataのアセットキー")]
        public string actorSetupDataAssetKey = "pl000";
        [Tooltip("バトルスキルマスターリスト")]
        public BattleSkillMaster[] skillMasters;

        /// <summary>アクター制御用SetupDataのアセットキー</summary>
        string IBattlePlayerMaster.ActorSetupDataAssetKey => actorSetupDataAssetKey;
        /// <summary>体力最大値</summary>
        int IBattlePlayerMaster.HealthMax => healthMax;
        /// <summary>スキルマスターリスト</summary>
        IReadOnlyList<IBattleSkillMaster> IBattlePlayerMaster.SkillMasters => skillMasters;
    }
}