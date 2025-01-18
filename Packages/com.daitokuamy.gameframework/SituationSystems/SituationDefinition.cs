using System;
using System.Collections;
using System.Linq;
using GameFramework.Core;

namespace GameFramework.SituationSystems {
    // 遷移ステータス
    public enum TransitionState {
        Invalid = -1,
        Standby, // 遷移待機状態
        Initializing, // 初期化中
        Opening, // オープン中
        Completed, // 遷移完了
        Canceled, // 遷移キャンセル
    }

    /// <summary>
    /// シチュエーション遷移確認用ハンドル
    /// </summary>
    public struct TransitionHandle : IProcess {
        /// <summary>無効なハンドル</summary>
        public static readonly TransitionHandle Empty = new TransitionHandle();
        
        // 制御用インスタンス
        private SituationContainer.TransitionInfo _transitionInfo;

        // 有効なハンドルか
        public bool IsValid => _transitionInfo != null;
        // 遷移完了か
        public bool IsDone => !IsValid || TransitionState == TransitionState.Completed ||
                              TransitionState == TransitionState.Canceled;
        // 例外
        public Exception Exception { get; private set; }
        // 遷移前のシチュエーション
        public Situation Prev => (Situation)_transitionInfo?.PrevSituations.FirstOrDefault();
        // 遷移後のシチュエーション
        public Situation Next => (Situation)_transitionInfo?.NextSituations.LastOrDefault();
        // 戻り遷移か
        public bool Back => _transitionInfo?.Back ?? false;
        // 遷移状態
        public TransitionState TransitionState => _transitionInfo?.State ?? TransitionState.Invalid;
        // IEnumerator用
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