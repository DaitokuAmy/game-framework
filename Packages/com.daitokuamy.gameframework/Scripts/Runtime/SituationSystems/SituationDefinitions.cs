using System;
using System.Collections;
using System.Linq;
using GameFramework.Core;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 遷移状態
    /// </summary>
    public enum TransitionState {
        /// <summary>無効値</summary>
        Invalid = -1,
        /// <summary>遷移待機状態</summary>
        Standby,
        /// <summary>初期化中</summary>
        Initializing,
        /// <summary>オープン中</summary>
        Opening,
        /// <summary>遷移完了</summary>
        Completed,
        /// <summary>遷移キャンセル</summary>
        Canceled,
    }
    
    /// <summary>
    /// 遷移ステップ
    /// </summary>
    public enum TransitionStep {
        /// <summary>読み込みまで</summary>
        Load,
        /// <summary>初期化まで</summary>
        Setup,
        /// <summary>完了まで</summary>
        Complete,
    }
    
    /// <summary>
    /// PreLoadの状態
    /// </summary>
    public enum PreLoadState {
        /// <summary>PreLoad無し</summary>
        None,
        /// <summary>PreLoad中</summary>
        PreLoading,
        /// <summary>PreLoad済み</summary>
        PreLoaded,
    }

    /// <summary>
    /// シチュエーション遷移確認用ハンドル
    /// </summary>
    public struct TransitionHandle : IProcess {
        /// <summary>無効なハンドル</summary>
        public static readonly TransitionHandle Empty = new TransitionHandle();
        
        // 制御用インスタンス
        private readonly SituationContainer.TransitionInfo _transitionInfo;

        /// <summary>有効なハンドルか</summary>
        public bool IsValid => _transitionInfo != null;
        /// <summary>遷移完了か</summary>
        public bool IsDone => !IsValid || TransitionState == TransitionState.Completed ||
                              TransitionState == TransitionState.Canceled;
        /// <summary>例外</summary>
        public Exception Exception { get; private set; }
        /// <summary>遷移前のシチュエーション</summary>
        public Situation Prev => (Situation)_transitionInfo?.PrevSituations.FirstOrDefault();
        /// <summary>遷移後のシチュエーション</summary>
        public Situation Next => (Situation)_transitionInfo?.NextSituations.LastOrDefault();
        /// <summary>戻り遷移か</summary>
        public bool Back => _transitionInfo != null && _transitionInfo.TransitionType == TransitionDirection.Back;
        /// <summary>遷移状態</summary>
        public TransitionState TransitionState => _transitionInfo?.State ?? TransitionState.Invalid;
        
        /// <summary>IEnumerator用</summary>
        object IEnumerator.Current => null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransitionHandle(SituationContainer.TransitionInfo info) {
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

        /// <summary>
        /// 遷移ステップを進める
        /// </summary>
        public void NextStep(TransitionStep step) {
            if (!IsValid) {
                return;
            }

            if (step <= _transitionInfo.Step) {
                return;
            }

            _transitionInfo.Step = step;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
    }
}