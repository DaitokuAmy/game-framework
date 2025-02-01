using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace GameFramework.CoroutineSystems {
    /// <summary>
    /// 並列実行用コルーチン
    /// </summary>
    public class MergedCoroutine : IEnumerator {
        private Coroutine[] _coroutines;

        // 現在の位置(未使用)
        public object Current => null;
        // 完了しているか
        public bool IsDone { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="funcs">非同期処理リスト</param>
        public MergedCoroutine(params IEnumerator[] funcs) {
            _coroutines = funcs.Select(x => new Coroutine(x))
                .ToArray();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="funcs">非同期処理リスト</param>
        public MergedCoroutine(IEnumerable<IEnumerator> funcs) {
            _coroutines = funcs.Select(x => new Coroutine(x))
                .ToArray();
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        public void Reset() {
            for (var i = 0; i < _coroutines.Length; i++) {
                var coroutine = (IEnumerator)_coroutines[i];
                coroutine?.Reset();
            }
        }

        /// <summary>
        /// コルーチン進行
        /// </summary>
        /// <returns>次の処理があるか？</returns>
        public bool MoveNext() {
            var finished = true;

            for (var i = 0; i < _coroutines.Length; i++) {
                var coroutine = _coroutines[i];

                if (coroutine == null) {
                    continue;
                }
                if (!coroutine.MoveNext()) {
                    _coroutines[i] = null;
                }
                else {
                    finished = false;
                }
            }

            IsDone = finished;
            return !IsDone;
        }
    }
}