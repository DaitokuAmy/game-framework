using UnityEngine.Profiling;

namespace GameFramework.PerformanceSystem {
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
        
        /// <inheritdoc/>
        bool IMemoryMonitor.CheckStartGC() {
            var size = Profiler.GetTotalAllocatedMemoryLong();
            return size >= _thresholdMemorySize;
        }
    }
}
