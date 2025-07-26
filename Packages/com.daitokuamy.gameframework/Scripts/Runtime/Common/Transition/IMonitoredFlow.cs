using System.Collections.Generic;

namespace GameFramework {
    /// <summary>
    /// モニタリング対象のIStateRouterにつけるインターフェース
    /// </summary>
    public interface IMonitoredStateRouter {
        /// <summary>表示名</summary>
        string Label { get; }
        /// <summary>戻り先の情報</summary>
        string BackStateInfo { get; }

        /// <summary>
        /// モニタリング用の詳細情報取得
        /// </summary>
        void GetDetails(List<(string label, string text)> lines);
    }
}