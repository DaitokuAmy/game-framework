using System;
using GameFramework;
using UnityEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトルテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "dat_battle_table.asset", menuName = "Sample Game/Table Data/Battle")]
    public sealed class BattleTableData : ScriptableTableData<int, BattleTableData.Element> {
        /// <summary>
        /// 要素
        /// </summary>
        [Serializable]
        public class Element : ITableElement<int> {
            [SerializeField, Tooltip("識別子")]
            private int _id;
            [SerializeField, Tooltip("名称")]
            private string _name;
            [SerializeField, Tooltip("読み込むフィールドのId")]
            private int _fieldId;

            /// <inheritdoc/>
            int ITableElement<int>.Id => _id;

            public int Id => _id;
            public string Name => _name;
            public int FieldId => _fieldId;
        }
    }
}