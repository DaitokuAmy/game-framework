using UnityEngine;

namespace GameFramework.BodySystems {
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
            Change(_defaultState);
        }

        /// <summary>
        /// ステートの変更
        /// </summary>
        /// <param name="stateName">ステート名</param>
        public abstract void Change(string stateName);

        /// <summary>
        /// 現在のステート名変更
        /// </summary>
        protected void SetCurrentStateName(string stateName) {
            CurrentStateName = stateName;
        }
    }
}