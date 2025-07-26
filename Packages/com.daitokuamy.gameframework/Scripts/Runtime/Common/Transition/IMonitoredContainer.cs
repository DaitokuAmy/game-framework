using System.Collections.Generic;

namespace GameFramework {
    /// <summary>
    /// モニタリング対象のIStateContainerにつけるインターフェース
    /// </summary>
    public interface IMonitoredStateContainer {
        /// <summary>
        /// 遷移情報
        /// </summary>
        public struct TransitionInfo {
            /// <summary>遷移向き</summary>
            public TransitionDirection Direction;
            /// <summary>遷移状態</summary>
            public TransitionState State;
            /// <summary>遷移終了ステップ</summary>
            public TransitionStep EndStep;
            /// <summary>遷移前のState情報</summary>
            public string PrevStateInfo;
            /// <summary>遷移後のState情報</summary>
            public string NextStateInfo;
        }
        
        /// <summary>表示名</summary>
        string Label { get; }
        /// <summary>遷移中か</summary>
        bool IsTransitioning { get; }
        /// <summary>現在のState情報</summary>
        string CurrentStateInfo { get; }

        /// <summary>
        /// 遷移情報の取得
        /// </summary>
        bool TryGetTransitionInfo(out TransitionInfo info);

        /// <summary>
        /// 詳細情報取得
        /// </summary>
        void GetDetails(List<(string label, string text)> lines);
    }
}