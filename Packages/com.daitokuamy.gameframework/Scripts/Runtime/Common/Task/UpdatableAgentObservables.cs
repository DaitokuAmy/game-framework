#if USE_R3
using System.Threading;
using R3;
#endif

namespace GameFramework {
    /// <summary>
    /// UpdatableAgentRx拡張
    /// </summary>
    public static class UpdatableAgentObservables {
#if USE_R3
        /// <summary>
        /// 更新タイミング監視
        /// </summary>
        public static Observable<Unit> UpdateAsObservable(this UpdatableAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.UpdateEvent += h, h => source.UpdateEvent -= h, ct);
        }

        /// <summary>
        /// 後更新タイミング監視
        /// </summary>
        public static Observable<Unit> LateUpdateAsObservable(this LateUpdatableAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.LateUpdateEvent += h, h => source.LateUpdateEvent -= h, ct);
        }

        /// <summary>
        /// 固定更新タイミング監視
        /// </summary>
        public static Observable<Unit> FixedUpdateAsObservable(this FixedUpdatableAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.FixedUpdateEvent += h, h => source.FixedUpdateEvent -= h, ct);
        }

        /// <summary>
        /// 更新タイミング監視
        /// </summary>
        public static Observable<Unit> UpdateAsObservable(this UpdateAndLateUpdatableAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.UpdateEvent += h, h => source.UpdateEvent -= h, ct);
        }

        /// <summary>
        /// 後更新タイミング監視
        /// </summary>
        public static Observable<Unit> LateUpdateAsObservable(this UpdateAndLateUpdatableAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.LateUpdateEvent += h, h => source.LateUpdateEvent -= h, ct);
        }

        /// <summary>
        /// 更新タイミング監視
        /// </summary>
        public static Observable<Unit> UpdateAsObservable(this FullUpdatableAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.UpdateEvent += h, h => source.UpdateEvent -= h, ct);
        }

        /// <summary>
        /// 後更新タイミング監視
        /// </summary>
        public static Observable<Unit> LateUpdateAsObservable(this FullUpdatableAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.LateUpdateEvent += h, h => source.LateUpdateEvent -= h, ct);
        }

        /// <summary>
        /// 固定更新タイミング監視
        /// </summary>
        public static Observable<Unit> FixedUpdateAsObservable(this FullUpdatableAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.FixedUpdateEvent += h, h => source.FixedUpdateEvent -= h, ct);
        }
#endif
    }
}
