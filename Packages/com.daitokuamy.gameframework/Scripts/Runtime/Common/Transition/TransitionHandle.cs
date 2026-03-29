using System;
using System.Collections;

namespace GameFramework {
    /// <summary>
    /// 遷移情報用インターフェース
    /// </summary>
    public interface ITransitionInfo<out TState>
        where TState : class {
        /// <summary>遷移向き</summary>
        TransitionDirection Direction { get; }
        /// <summary>遷移状態</summary>
        TransitionState State { get; }
        /// <summary>遷移失敗時の例外</summary>
        Exception Exception { get; }
        /// <summary>遷移前のState</summary>
        TState Prev { get; }
        /// <summary>遷移後のState</summary>
        TState Next { get; }

        /// <summary>終了通知</summary>
        event Action<TState> FinishedEvent;
    }

    /// <summary>
    /// 遷移確認用ハンドル
    /// </summary>
    public struct TransitionHandle<TState> : IEventProcess<TState>
        where TState : class {
        /// <summary>無効なハンドル</summary>
        public static readonly TransitionHandle<TState> Empty = new();

        private readonly ITransitionInfo<TState> _transitionInfo;

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <summary>終了通知</summary>
        event Action<TState> IEventProcess<TState>.ExitEvent {
            add {
                if (_transitionInfo != null) {
                    _transitionInfo.FinishedEvent += value;
                }
            }
            remove {
                if (_transitionInfo != null) {
                    _transitionInfo.FinishedEvent -= value;
                }
            }
        }

        /// <summary>結果</summary>
        public TState Result => Next;
        /// <summary>有効なハンドルか</summary>
        public bool IsValid => _transitionInfo != null;
        /// <summary>遷移完了か</summary>
        public bool IsDone => !IsValid || TransitionState == TransitionState.Completed ||
                              TransitionState == TransitionState.Canceled;
        /// <summary>例外</summary>
        public Exception Exception => _transitionInfo?.Exception ?? _exception;
        /// <summary>遷移前のState</summary>
        public TState Prev => _transitionInfo?.Prev;
        /// <summary>遷移後のState</summary>
        public TState Next => _transitionInfo?.Next;
        /// <summary>遷移向き</summary>
        public TransitionDirection Direction => _transitionInfo?.Direction ?? TransitionDirection.Forward;
        /// <summary>戻り遷移か</summary>
        public bool IsBack => Direction == TransitionDirection.Back;
        /// <summary>遷移状態</summary>
        public TransitionState TransitionState => _transitionInfo?.State ?? TransitionState.Invalid;

        private readonly Exception _exception;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransitionHandle(ITransitionInfo<TState> info) {
            _transitionInfo = info;
            _exception = null;
        }

        /// <summary>
        /// コンストラクタ(エラー用)
        /// </summary>
        public TransitionHandle(Exception exception) {
            _transitionInfo = null;
            _exception = exception;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public EventProcessAwaiter<TState> GetAwaiter() {
            return new EventProcessAwaiter<TState>(this);
        }
    }
}
