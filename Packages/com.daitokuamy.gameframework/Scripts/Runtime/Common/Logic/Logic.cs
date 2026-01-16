using System;
using System.Threading;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// ロジック処理
    /// </summary>
    public abstract class Logic : IDisposable, IScope {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        
        private DisposableScope _activeScope;
        private bool _disposed;

        /// <summary>アクティブ状態</summary>
        public virtual bool IsActive => _activeScope != null;
        /// <summary>廃棄済みか</summary>
        public bool IsDisposed => _disposed;
        /// <summary>キャンセル用トークン</summary>
        public CancellationToken Token => _cancellationTokenSource.Token;
        
        /// <summary>Scope用</summary>
        public event Action ExpiredEvent;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            // Deactivateを実行
            Deactivate();

            DisposeInternal();
            SystemDisposeInternal();
            ExpiredEvent?.InvokeDescending();
            ExpiredEvent = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// システム用の廃棄処理
        /// </summary>
        private protected virtual void SystemDisposeInternal() {
        }

        /// <summary>
        /// 解放処理(Override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// アクティブ時処理(Override用)
        /// </summary>
        protected virtual void ActivateInternal(IScope scope) {
        }

        /// <summary>
        /// 非アクティブ時処理(Override用)
        /// </summary>
        protected virtual void DeactivateInternal() {
        }

        /// <summary>
        /// 有効化
        /// </summary>
        public void Activate() {
            if (_activeScope != null || _disposed) {
                return;
            }

            _activeScope = new DisposableScope();
            ActivateInternal(_activeScope);
        }

        /// <summary>
        /// 無効化
        /// </summary>
        public void Deactivate() {
            if (_activeScope == null) {
                return;
            }

            DeactivateInternal();
            _activeScope.Dispose();
            _activeScope = null;
        }
    }
}