using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// ステートを変更するGimmickの基底
    /// </summary>
    public abstract class StateGimmickBase<T> : StateGimmick
        where T : StateGimmickBase<T>.StateInfoBase {
        /// <summary>
        /// ステート情報基底
        /// </summary>
        [Serializable]
        public class StateInfoBase {
            [Tooltip("ステート名")]
            public string stateName;
        }

        [SerializeField, Tooltip("ステート情報リスト")]
        private T[] _stateInfos;

        private readonly Dictionary<string, T> _stateInfoDict = new();

        /// <summary>ステート名一覧</summary>
        public override string[] StateNames => _stateInfoDict.Keys.ToArray();
        /// <summary>ステート一覧</summary>
        protected IReadOnlyList<T> StateInfos => _stateInfos;

        /// <summary>
        /// ステートの変更
        /// </summary>
        /// <param name="stateName">ステート名</param>
        /// <param name="immediate">即時遷移するか</param>
        public sealed override void Change(string stateName, bool immediate = false) {
            if (stateName == CurrentStateName) {
                return;
            }

            var prev = FindStateInfo(CurrentStateName);
            var current = FindStateInfo(stateName);
            ChangeState(prev, current, immediate);
            SetCurrentStateName(stateName);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            foreach (var stateInfo in _stateInfos) {
                _stateInfoDict[stateInfo.stateName] = stateInfo;
            }
        }

        /// <summary>
        /// ステートの変更処理
        /// </summary>
        /// <param name="prev">変更前のステート</param>
        /// <param name="current">変更後のステート</param>
        /// <param name="immediate">即時遷移するか</param>
        protected abstract void ChangeState(T prev, T current, bool immediate);

        /// <summary>
        /// ステートの検索
        /// </summary>
        protected T FindStateInfo(string stateName) {
            if (string.IsNullOrEmpty(stateName)) {
                return null;
            }

            _stateInfoDict.TryGetValue(stateName, out var stateInfo);
            return stateInfo;
        }
    }
}