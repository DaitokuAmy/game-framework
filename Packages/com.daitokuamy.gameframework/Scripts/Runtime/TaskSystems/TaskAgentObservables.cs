#if USE_R3
using System.Threading;
using R3;

#elif USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework.TaskSystems {
    /// <summary>
    /// TaskAgentRx拡張
    /// </summary>
    public static class TaskAgentObservables {
#if USE_R3
        /// <summary>
        /// 更新タイミング監視
        /// </summary>
        public static Observable<Unit> UpdateAsObservable(this TaskAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.UpdateEvent += h, h => source.UpdateEvent -= h, ct);
        }

        /// <summary>
        /// 後更新タイミング監視
        /// </summary>
        public static Observable<Unit> LateUpdateAsObservable(this TaskAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.LateUpdateEvent += h, h => source.LateUpdateEvent -= h, ct);
        }

        /// <summary>
        /// 固定更新タイミング監視
        /// </summary>
        public static Observable<Unit> FixedUpdateAsObservable(this FixedTaskAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.FixedUpdateEvent += h, h => source.FixedUpdateEvent -= h, ct);
        }
#elif USE_UNI_RX
        /// <summary>
        /// 更新タイミング監視
        /// </summary>
        public static IObservable<Unit> UpdateAsObservable(this TaskAgent source) {
            return Observable.FromEvent(h => source.UpdateEvent += h, h => source.UpdateEvent -= h);
        }

        /// <summary>
        /// 後更新タイミング監視
        /// </summary>
        public static IObservable<Unit> LateUpdateAsObservable(this TaskAgent source) {
            return Observable.FromEvent(h => source.LateUpdateEvent += h, h => source.LateUpdateEvent -= h);
        }

        /// <summary>
        /// 固定更新タイミング監視
        /// </summary>
        public static IObservable<Unit> FixedUpdateAsObservable(this FixedTaskAgent source) {
            return Observable.FromEvent(h => source.FixedUpdateEvent += h, h => source.FixedUpdateEvent -= h);
        }
#endif
    }
}
