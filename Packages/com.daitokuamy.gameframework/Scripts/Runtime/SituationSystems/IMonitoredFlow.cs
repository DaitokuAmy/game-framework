using System.Collections.Generic;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// モニタリング対象につけるインターフェース
    /// </summary>
    public interface IMonitoredFlow {
        /// <summary>表示名</summary>
        string Label { get; }
        /// <summary>戻り先のSituation</summary>
        Situation BackTarget { get; }

        /// <summary>
        /// モニタリング用の詳細情報取得
        /// </summary>
        void GetDetails(List<(string label, string text)> lines);
    }
}