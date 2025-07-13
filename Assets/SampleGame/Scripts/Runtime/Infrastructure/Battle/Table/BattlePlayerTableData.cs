using System;
using GameFramework;
using SampleGame.Domain.Battle;
using UnityEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトル用プレイヤーテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "dat_battle_player_table.asset", menuName = "Sample Game/Table Data/Battle Player")]
    public sealed class BattlePlayerTableData : ScriptableTableData<int, BattlePlayerTableData.Element> {
        /// <summary>
        /// 要素
        /// </summary>
        [Serializable]
        public class Element : ITableElement<int>, IPlayerMaster {
            [SerializeField, Tooltip("識別子")]
            private int _id;
            [SerializeField, Tooltip("名称")]
            private string _name;
            [SerializeField, Tooltip("アセットキー")]
            private string _assetKey;
            [SerializeField, Tooltip("アクター制御アセットキー")]
            private string _actorAssetKey;

            /// <inheritdoc/>
            int ITableElement<int>.Id => _id;
            /// <inheritdoc/>
            int ICharacterMaster.Id => _id;
            /// <inheritdoc/>
            string ICharacterMaster.Name => _name;
            /// <inheritdoc/>
            string ICharacterMaster.AssetKey => _assetKey;
            /// <inheritdoc/>
            string ICharacterMaster.ActorAssetKey => _actorAssetKey;
        }
    }
}