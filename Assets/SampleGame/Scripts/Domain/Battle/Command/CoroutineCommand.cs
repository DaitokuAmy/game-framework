using System.Collections;
using GameFramework.CommandSystems;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// コルーチンを実行するコマンド
    /// </summary>
    public abstract class CoroutineCommand : Command {
        private CoroutineRunner _coroutineRunner = new();
        private bool _finished;
        
        /// <summary>
        /// 開始処理
        /// </summary>
        protected override void StartInternal(IScope scope) {
            base.StartInternal(scope);

            _coroutineRunner.StartCoroutine(ExecuteRoutine(scope), () => {
                _finished = true;
            }, () => {
                _finished = true;
            }, error => {
                Debug.LogException(error);
                _finished = true;
            });
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override bool UpdateInternal() {
            _coroutineRunner.Update();
            return !_finished;
        }

        /// <summary>
        /// 実行ルーチン
        /// </summary>
        protected abstract IEnumerator ExecuteRoutine(IScope scope);
    }
}