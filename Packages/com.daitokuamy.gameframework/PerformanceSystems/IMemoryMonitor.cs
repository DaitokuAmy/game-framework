namespace GameFramework.PerformanceSystems {
    /// <summary>
    /// メモリモニタリング用インターフェース
    /// </summary>
    public interface IMemoryMonitor {
        /// <summary>
        /// GCを開始するかのチェック
        /// </summary>
        /// <returns>GCを開始するか</returns>
        bool CheckStartGC();
    }
}
