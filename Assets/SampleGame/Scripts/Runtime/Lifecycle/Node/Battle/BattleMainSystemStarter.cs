using System;
using GameFramework.NavigationSystems;
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
        protected override Type NavNodeType => typeof(BattleSessionNode);

        /// <inheritdoc/>
        protected override void OnNodeSetup(INavNode navNode) {
            if (navNode is BattleSessionNode battleSceneSituation) {
                battleSceneSituation.Setup(_battleId, _playerId);
            }
        }
    }
}