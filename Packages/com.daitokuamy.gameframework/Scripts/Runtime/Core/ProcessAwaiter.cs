using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GameFramework.Core {
    /// <summary>
    /// IEventProcess用のAwaiter
    /// </summary>
    public struct EventProcessAwaiter : INotifyCompletion {
        private readonly IEventProcess _process;

        private Action _continuation;

        /// <summary>完了しているか</summary>
        public bool IsCompleted {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _process.IsDone;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventProcessAwaiter(IEventProcess process) {
            _process = process;
            _continuation = null;
        }

        /// <summary>
        /// Await開始時の処理
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation) {
            if (_process.IsDone) {
                continuation.Invoke();
                return;
            }

            _continuation = continuation;
            _process.ExitEvent += Invoke;
        }

        /// <summary>
        /// 結果の取得
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult() {
        }

        /// <summary>
        /// 発火時処理
        /// </summary>
        private void Invoke() {
            _continuation?.Invoke();
            _continuation = null;
            _process.ExitEvent -= Invoke;
        }
    }

    /// <summary>
    /// IEventProcess<T>用のAwaiter
    /// </summary>
    public struct EventProcessAwaiter<T> : INotifyCompletion {
        private readonly IEventProcess<T> _process;

        private Action _continuation;

        /// <summary>完了しているか</summary>
        public bool IsCompleted {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _process.IsDone;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventProcessAwaiter(IEventProcess<T> process) {
            _process = process;
            _continuation = null;
        }

        /// <summary>
        /// Await開始時の処理
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation) {
            if (_process.IsDone) {
                continuation.Invoke();
                return;
            }

            _continuation = continuation;
            _process.ExitEvent += Invoke;
        }

        /// <summary>
        /// 結果の取得
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetResult() {
            return _process.Result;
        }

        /// <summary>
        /// 発火時処理
        /// </summary>
        private void Invoke(T _) {
            _continuation?.Invoke();
            _continuation = null;
            _process.ExitEvent -= Invoke;
        }
    }
}