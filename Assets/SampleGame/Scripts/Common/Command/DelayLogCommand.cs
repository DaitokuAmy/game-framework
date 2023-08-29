using GameFramework.CommandSystems;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 一定時間後にログを出力するためのサンプル用コマンド
    /// </summary>
    public class DelayLogCommand : Command {
        private LayeredTime _layeredTime;
        private float _timer;
        private string _log = "";

        public override int Priority { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DelayLogCommand(string log, float delay, int priority = 0, LayeredTime layeredTime = null) {
            Priority = priority;
            _log = log;
            _timer = delay;
            _layeredTime = layeredTime;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override bool UpdateInternal() {
            var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Time.deltaTime;
            if (_timer <= 0.0f) {
                Debug.Log(_log);
                return false;
            }

            _timer -= deltaTime;
            return true;
        }
    }
}