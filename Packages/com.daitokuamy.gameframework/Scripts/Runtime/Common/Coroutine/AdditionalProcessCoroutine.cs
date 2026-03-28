using System;
using System.Collections;

namespace GameFramework {
    /// <summary>
    /// コルーチンに追加処理を行う機能
    /// </summary>
    public class AdditionalProcessCoroutine : IEnumerator {
        // ベースのコルーチン処理
        private Coroutine _baseCoroutine;
        // 追記処理
        private Action _additionalFunc;

        // 現在の位置(未使用)
        object IEnumerator.Current => ((IEnumerator)_baseCoroutine).Current;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="baseCoroutine">ベースのコルーチン処理</param>
        /// <param name="additionalFunc">追記処理</param>
        public AdditionalProcessCoroutine(IEnumerator baseCoroutine, Action additionalFunc) {
            _baseCoroutine = new Coroutine(baseCoroutine);
            _additionalFunc = additionalFunc;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            ((IEnumerator)_baseCoroutine).Reset();
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            if (_baseCoroutine.MoveNext()) {
                _additionalFunc?.Invoke();
                return true;
            }

            return false;
        }
    }
}
