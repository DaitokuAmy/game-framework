using GameFramework.CommandSystems;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// ログを出力するためのサンプル用コマンド
    /// </summary>
    public class LogCommand : Command {
        private string _log = "";
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LogCommand(string log) {
            _log = log;
        }
        
        /// <summary>
        /// 開始処理
        /// </summary>
        protected override void StartInternal(IScope scope) {
            Debug.Log(_log);
        }
    }
}