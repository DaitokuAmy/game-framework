using SampleGame.Domain.Battle;
using UnityEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトル用のプレイヤーマスター
    /// </summary>
    [CreateAssetMenu(fileName = "dat_master_battle_player_pl000.asset", menuName = "SampleGame/Master Data/Battle Player")]
    public class BattlePlayerMasterData : ScriptableObject, IBattlePlayerMasterData {
        [Tooltip("体力の最大値")]
        public int healthMax = 100;
        [Tooltip("アクター制御用SetupDataのアセットキー")]
        public string actorSetupDataAssetKey = "pl000";

        /// <summary>アクター制御用SetupDataのアセットキー</summary>
        public string ActorSetupDataAssetKey => actorSetupDataAssetKey;
        /// <summary>体力最大値</summary>
        public int HealthMax => healthMax;
    }
}