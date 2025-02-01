using UnityEngine.Profiling;

namespace GameFramework.PerformanceSystems {
    /// <summary>
    /// デフォルトのメモリモニタリングクラス
    /// </summary>
    public class DefaultMemoryMonitor : IMemoryMonitor {
        // GCを発生させる境界値
        private readonly long _thresholdMemorySize;
    
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="thresholdMemorySize">GCを発生させる境界値</param>
        public DefaultMemoryMonitor(long thresholdMemorySize) {
            _thresholdMemorySize = thresholdMemorySize;
        }
        
        /// <summary>
        /// GCを開始するかのチェック
        /// </summary>
        /// <returns>GCを開始するか</returns>
        bool IMemoryMonitor.CheckStartGC() {
            var size = Profiler.GetTotalAllocatedMemoryLong();
            return size >= _thresholdMemorySize;
        }
    }
}
