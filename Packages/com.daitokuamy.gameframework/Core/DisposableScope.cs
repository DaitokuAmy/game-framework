using System;
using System.Threading;

namespace GameFramework.Core {
    /// <summary>
    /// スコープ管理用インターフェース
    /// </summary>
    public class DisposableScope : IScope, IDisposable {
        private CancellationTokenSource _cancellationTokenSource = new();
        
        // 廃棄済みか
        public bool Disposed { get; private set; }
        // キャンセルハンドリング用トークン
        public CancellationToken Token => _cancellationTokenSource.Token;

        // スコープ終了通知
        public event Action OnExpired;

        /// <summary>
        /// クリア処理
        /// </summary>
        public void Clear() {
            if (Disposed) {
                return;
            }

            OnExpired?.Invoke();
            OnExpired = null;
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
            OnExpired?.Invoke();
            OnExpired = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
}