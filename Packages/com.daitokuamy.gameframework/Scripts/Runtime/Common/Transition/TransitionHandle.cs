using System;
using System.Collections;
using GameFramework.Core;

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
        /// <summary>遷移終了ステップ</summary>
        TransitionStep EndStep { get; }
        /// <summary>遷移前のState</summary>
        TState Prev { get; }
        /// <summary>遷移後のState</summary>
        TState Next { get; }

        /// <summary>終了通知</summary>
        event Action<TState> FinishedEvent;

        /// <summary>
        /// 終了ステップの変更
        /// </summary>
        bool ChangeEndStep(TransitionStep step);
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
        public Exception Exception { get; private set; }
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransitionHandle(ITransitionInfo<TState> info) {
            _transitionInfo = info;
            Exception = null;
        }

        /// <summary>
        /// コンストラクタ(エラー用)
        /// </summary>
        public TransitionHandle(Exception exception) {
            _transitionInfo = null;
            Exception = exception;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 遷移ステップを進める
        /// </summary>
        public bool NextStep(TransitionStep step) {
            if (!IsValid) {
                return false;
            }

            if (step <= _transitionInfo.EndStep) {
                return false;
            }

            return _transitionInfo.ChangeEndStep(step);
        }
    }
}