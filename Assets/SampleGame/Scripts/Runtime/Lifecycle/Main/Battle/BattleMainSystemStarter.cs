using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム起動用のStarter
    /// </summary>
    public sealed class BattleMainSystemStarter : MainSystemStarter {
        [SerializeField, Tooltip("開始バトルId")]
        private int _battleId = 1;
        [SerializeField, Tooltip("開始プレイヤーId")]
        private int _playerId = 1;
        
        protected override ISituationSetup GetSituationSetup() {
            return new SituationSetup<BattleSceneSituation>(situation => {
                situation.Setup(_battleId, _playerId);
            });
        }
    }
}