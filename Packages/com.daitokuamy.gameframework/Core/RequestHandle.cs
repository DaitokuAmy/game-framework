using System;
using System.Collections;

namespace GameFramework.Core {
    /// <summary>
    /// リクエストを受けるためのインターフェース
    /// </summary>
    public interface IRequestReceiver<T> {
        /// <summary>受信を開始しているか</summary>
        bool IsStarted { get; }
        /// <summary>受信が完了しているか</summary>
        bool IsDone { get; }

        /// <summary>
        /// リクエストを受け取った時の処理
        /// </summary>
        void OnRequested(T request, params object[] args);
    }

    /// <summary>
    /// リクエストを送るためのハンドル
    /// </summary>
    public readonly struct RequestHandle<T> : IProcess {
        private readonly IRequestReceiver<T> _receiver;

        object IEnumerator.Current => null;
        Exception IProcess.Exception => null;

        /// <summary>受信開始しているか</summary>
        public bool IsStarted => _receiver != null && _receiver.IsStarted;
        /// <summary>受信が完了しているか</summary>
        public bool IsDone => _receiver == null || !_receiver.IsDone;

        /// <summary>
        /// Coroutine用の完了チェック
        /// </summary>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <summary>
        /// リセットは未使用
        /// </summary>
        void IEnumerator.Reset() {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RequestHandle(IRequestReceiver<T> receiver) {
            _receiver = receiver;
        }

        /// <summary>
        /// リクエストを送る
        /// </summary>
        public void Request(T request, params object[] args) {
            if (_receiver == null || _receiver.IsDone) {
                return;
            }

            _receiver.OnRequested(request, args);
        }
    }
}