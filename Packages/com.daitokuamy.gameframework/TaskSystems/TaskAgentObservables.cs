#if USE_UNI_RX

using System;
using UniRx;

namespace GameFramework.TaskSystems {
    /// <summary>
    /// TaskAgentRx拡張
    /// </summary>
    public static class TaskAgentObservables {
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
    }
}

#endif // USE_UNI_RX