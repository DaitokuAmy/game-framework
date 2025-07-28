using System;
using System.Threading;

namespace GameFramework.Core {
    /// <summary>
    /// スコープ管理用インターフェース
    /// </summary>
    public class DisposableScope : IScope, IDisposable {
        private CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>廃棄済みか</summary>
        public bool Disposed { get; private set; }
        /// <summary>キャンセルハンドリング用トークン</summary>
        public CancellationToken Token => _cancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>スコープ終了通知</summary>
        public event Action ExpiredEvent;

        /// <summary>
        /// クリア処理
        /// </summary>
        public void Clear() {
            if (Disposed) {
                return;
            }
            
            ExpiredEvent?.InvokeDescending();
            ExpiredEvent = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (Disposed) {
                return;
            }

            Disposed = true;
            ExpiredEvent?.InvokeDescending();
            ExpiredEvent = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
}