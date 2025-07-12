using System;
using System.Threading;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// ロジック処理
    /// </summary>
    public abstract class Logic : ILateUpdatableTask, IDisposable, IScope, ITaskEventHandler {
        // キャンセル用
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        
        // アクティブスコープ
        private DisposableScope _activeScope;
        // 廃棄済みフラグ
        private bool _disposed;
        // タスクを回してるRunner
        private TaskRunner _taskRunner;

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
            ExpiredEvent?.InvokeDescending();
            ExpiredEvent = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            // Taskから登録を除外
            if (_taskRunner != null) {
                _taskRunner.Unregister(this);
                _taskRunner = null;
            }
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

        /// <summary>
        /// タスク更新処理
        /// </summary>
        void ITask.Update() {
            UpdateInternal();
        }

        /// <summary>
        /// タスク後更新処理
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            LateUpdateInternal();
        }

        /// <summary>
        /// タスク登録時
        /// </summary>
        void ITaskEventHandler.OnRegistered(TaskRunner runner) {
            _taskRunner = runner;
        }

        /// <summary>
        /// タスク登録解除時
        /// </summary>
        void ITaskEventHandler.OnUnregistered(TaskRunner runner) {
            if (runner == _taskRunner) {
                _taskRunner = null;
            }
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
        /// 更新処理(Override用)
        /// </summary>
        protected virtual void UpdateInternal() {
        }

        /// <summary>
        /// 後更新処理(Override用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }
    }
}