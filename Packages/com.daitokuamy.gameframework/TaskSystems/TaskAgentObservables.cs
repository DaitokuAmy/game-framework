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
        public static Observable<Unit> OnUpdateAsObservable(this TaskAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.OnUpdate += h, h => source.OnUpdate -= h, ct);
        }

        /// <summary>
        /// 後更新タイミング監視
        /// </summary>
        public static Observable<Unit> OnLateUpdateAsObservable(this TaskAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.OnLateUpdate += h, h => source.OnLateUpdate -= h, ct);
        }

        /// <summary>
        /// 固定更新タイミング監視
        /// </summary>
        public static Observable<Unit> OnFixedUpdateAsObservable(this FixedTaskAgent source, CancellationToken ct = default) {
            return Observable.FromEvent(h => source.OnFixedUpdate += h, h => source.OnFixedUpdate -= h, ct);
        }
#elif USE_UNI_RX
        /// <summary>
        /// 更新タイミング監視
        /// </summary>
        public static IObservable<Unit> OnUpdateAsObservable(this TaskAgent source) {
            return Observable.FromEvent(h => source.OnUpdate += h, h => source.OnUpdate -= h);
        }

        /// <summary>
        /// 後更新タイミング監視
        /// </summary>
        public static IObservable<Unit> OnLateUpdateAsObservable(this TaskAgent source) {
            return Observable.FromEvent(h => source.OnLateUpdate += h, h => source.OnLateUpdate -= h);
        }

        /// <summary>
        /// 固定更新タイミング監視
        /// </summary>
        public static IObservable<Unit> OnFixedUpdateAsObservable(this FixedTaskAgent source) {
            return Observable.FromEvent(h => source.OnFixedUpdate += h, h => source.OnFixedUpdate -= h);
        }
#endif
    }
}
