using System;
using System.Collections;
using UnityEngine.Scripting;

namespace GameFramework.PerformanceSystems {
    /// <summary>
    /// メモリ管理
    /// </summary>
    public class MemoryManager : DisposableTask {
        // コルーチン実行制御用
        private CoroutineRunner _coroutineRunner;
        // IncrementalGCを実行するns
        private ulong _incrementalNanoSeconds;
        // IncrementalGCを連続実行し続ける最大フレーム数
        private int _incrementalFrameMax;
        // Incremental実行中コルーチン
        private Coroutine _coroutine;
        // メモリ監視用
        private IMemoryMonitor _memoryMonitor;

        // Incremental GCを使うか
#if UNITY_EDITOR
        private bool UseIncremental => false;
#else
        private bool UseIncremental => GarbageCollector.isIncremental;
#endif

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MemoryManager(ulong incrementalNanoSeconds, int incrementalFrameMax) {
            _coroutineRunner = new CoroutineRunner();
            _incrementalNanoSeconds = incrementalNanoSeconds;
            _incrementalFrameMax = incrementalFrameMax;
        }
        
        /// <summary>
        /// メモリ監視用のクラスを設定
        /// </summary>
        public void SetMonitor(IMemoryMonitor monitor) {
            _memoryMonitor = monitor;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            if (_coroutine != null) {
                _coroutineRunner.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        /// <summary>
        /// GCの実行
        /// </summary>
        /// <param name="immediate">即時実行するか</param>
        public void StartGC(bool immediate = false) {
            if (UseIncremental) {
                // IncrementalGCの停止
                if (_coroutine != null) {
                    _coroutineRunner.StopCoroutine(_coroutine);
                    _coroutine = null;
                }

                if (immediate) {
                    // GCの即時実行
                    GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                    GC.Collect();
                    GarbageCollector.GCMode = GarbageCollector.Mode.Manual;
                }
                else {
                    _coroutine = _coroutineRunner.StartCoroutine(StartIncrementalGCRoutine());
                }
            }
            else {
                GC.Collect();
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            // Monitorチェック
            if (_memoryMonitor != null) {
                if (_memoryMonitor.CheckStartGC()) {
                    StartGC();
                }
            }
            
            _coroutineRunner.Update();
        }

        /// <summary>
        /// IncrementalGCの実行
        /// </summary>
        private IEnumerator StartIncrementalGCRoutine() {
            var count = 0;
            // IncrementalGCの実行
            while (GarbageCollector.CollectIncremental(_incrementalNanoSeconds)) {
                count++;
                // 一定フレーム以上、解消しきれなかった場合に強制GCを行う
                if (count >= _incrementalFrameMax) {
                    StartGC(true);
                    yield break;
                }

                yield return null;
            }
        }
    }
}