using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GameFramework.Core {
    /// <summary>
    /// IEventProcess用のAwaiter
    /// </summary>
    public readonly struct EventProcessAwaiter : INotifyCompletion {
        private readonly IEventProcess _process;

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

            _process.ExitEvent += continuation;
        }

        /// <summary>
        /// 結果の取得
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult() {
        }
    }
    
    /// <summary>
    /// IEventProcess<T>用のAwaiter
    /// </summary>
    public readonly struct EventProcessAwaiter<T> : INotifyCompletion {
        private readonly IEventProcess<T> _process;

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

            _process.OnExitEvent += _ => continuation.Invoke();
        }

        /// <summary>
        /// 結果の取得
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetResult() {
            return _process.Result;
        }
    }
}