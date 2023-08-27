using UnityEngine;
using UnityEngine.Serialization;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用のプレイヤーマスター
    /// </summary>
    [CreateAssetMenu(fileName = "dat_battle_player_master_pl000.asset",
        menuName = "SampleGame/Battle Player Master Data")]
    public class BattlePlayerMasterData : ScriptableObject {
        public string characterName = "";
        public string assetKey = "pl000";
        public int healthMax = 100;
        [FormerlySerializedAs("playerActorSetupDataId")]
        public string actorSetupDataId = "pl000";
        public string[] playerActorGeneralActionDataIds;
    }
}