using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// コルーチン本体
    /// </summary>
    public class Coroutine : IProcess {
        private IEnumerator _enumerator;
        private Stack<object> _stack;
        private object _current;
        private bool _isDone;

        /// <summary>完了しているか</summary>
        public bool IsDone => _isDone || Exception != null;
        /// <summary>エラー内容</summary>
        public Exception Exception { get; private set; }
        
        /// <summary>現在の処理位置</summary>
        object IEnumerator.Current => _current;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="enumerator">処理</param>
        public Coroutine(IEnumerator enumerator) {
            _enumerator = enumerator;
            _stack = new Stack<object>();
            ((IEnumerator)this).Reset();
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        void IEnumerator.Reset() {
            _stack.Clear();
            _stack.Push(_enumerator);
            _current = null;
            _isDone = false;
        }

        /// <summary>
        /// IEnumeratorのコルーチン処理(更新しない)
        /// </summary>
        /// <returns>次の処理があるか？</returns>
        bool IEnumerator.MoveNext() {
            // Unity経由などでCoroutine実行されてしまう可能性があるため更新はしない
            return !IsDone;
        }

        /// <summary>
        /// CoroutineRunner用のコルーチン実行処理(更新あり)
        /// </summary>
        /// <returns>次の処理があるか？</returns>
        public bool MoveNext() {
            Update();
            return !IsDone;
        }

        /// <summary>
        /// 処理更新
        /// </summary>
        private void Update() {
            // 完了処理
            void Done() {
                _stack.Clear();
                _current = null;
                _isDone = true;
            }

            // スタックがなければ、完了
            if (_stack.Count == 0) {
                Done();
                return;
            }

            // スタックを取り出して、処理を進める
            var peek = _stack.Peek();
            _current = peek;

            try {
                if (peek == null) {
                    _stack.Pop();
                }
                else if (peek is Coroutine coroutine) {
                    if (coroutine.MoveNext()) {
                        _stack.Push(coroutine._current);
                    }
                    else {
                        _stack.Pop();
                    }

                    Update();
                }
                else if (peek is IEnumerator enumerator) {
                    if (enumerator.MoveNext()) {
                        _stack.Push(enumerator.Current);
                    }
                    else {
                        _stack.Pop();
                    }

                    Update();
                }
                else if (peek is AsyncOperation asyncOperation) {
                    if (asyncOperation.isDone) {
                        _stack.Pop();
                        Update();
                    }
                }
                else if (peek is WaitForSeconds waitForSeconds) {
                    _stack.Pop();
                    _stack.Push(waitForSeconds.GetEnumerator());
                    Update();
                }
                else {
                    throw new NotSupportedException($"{peek.GetType()} is not supported.");
                }
            }
            catch (Exception exception) {
                Exception = exception;
                throw;
            }
        }
    }
}