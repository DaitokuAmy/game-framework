using System.Collections.Generic;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// モニタリング対象につけるインターフェース
    /// </summary>
    public interface IMonitoredContainer {
        /// <summary>表示名</summary>
        string Label { get; }
        /// <summary>遷移中か</summary>
        bool IsTransitioning { get; }
        /// <summary>現在の遷移中情報</summary>
        SituationContainer.TransitionInfo CurrentTransitionInfo { get; }
        /// <summary>現在のシチュエーション</summary>
        Situation Current { get; }
        /// <summary>RootとなるSituation</summary>
        Situation RootSituation { get; }
        /// <summary>プリロード中のSituationリスト</summary>
        IReadOnlyList<Situation> PreloadSituations { get; }
        /// <summary>実行中のSituationリスト</summary>
        IReadOnlyList<Situation> RunningSituations { get; }
    }
}