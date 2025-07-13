using System;
using GameFramework;
using SampleGame.Domain.Battle;
using UnityEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// フィールドテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "dat_battle_field_table.asset", menuName = "Sample Game/Table Data/Battle Field")]
    public sealed class BattleFieldTableData : ScriptableTableData<int, BattleFieldTableData.Element> {
        /// <summary>
        /// 要素
        /// </summary>
        [Serializable]
        public class Element : ITableElement<int>, IFieldMaster {
            [SerializeField, Tooltip("識別子")]
            private int _id;
            [SerializeField, Tooltip("名称")]
            private string _name;
            [SerializeField, Tooltip("アセットキー")]
            private string _assetKey;

            /// <inheritdoc/>
            int ITableElement<int>.Id => _id;
            /// <inheritdoc/>
            int IFieldMaster.Id => _id;
            /// <inheritdoc/>
            string IFieldMaster.Name => _name;
            /// <inheritdoc/>
            string IFieldMaster.AssetKey => _assetKey;
        }
    }
}