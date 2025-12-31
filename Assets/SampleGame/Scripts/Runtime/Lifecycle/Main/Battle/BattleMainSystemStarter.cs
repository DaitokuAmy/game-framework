using System;
using GameFramework.SituationSystems;
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

        /// <inheritdoc/>
        protected override Type SituationType => typeof(BattleSceneSituation);

        /// <inheritdoc/>
        protected override void OnSituationSetup(Situation situation) {
            if (situation is BattleSceneSituation battleSceneSituation) {
                battleSceneSituation.Setup(_battleId, _playerId);
            }
        }
    }
}