using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// ステートを変更するGimmickの基底
    /// </summary>
    public abstract class StateGimmick : Gimmick {
        [SerializeField, Tooltip("デフォルトステート")]
        private string _defaultState = "Default";
        
        /// <summary>ステート名一覧</summary>
        public abstract string[] StateNames { get; }
        /// <summary>現在のステート名</summary>
        public string CurrentStateName { get; private set; } = "";

        /// <summary>
        /// 初期化後処理
        /// </summary>
        protected override void PostInitializeInternal() {
            Change(_defaultState, true);
        }

        /// <summary>
        /// ステートの変更
        /// </summary>
        /// <param name="stateName">ステート名</param>
        /// <param name="immediate">即時遷移するか</param>
        public abstract void Change(string stateName, bool immediate = false);

        /// <summary>
        /// 現在のステート名変更
        /// </summary>
        protected void SetCurrentStateName(string stateName) {
            CurrentStateName = stateName;
        }
    }
}