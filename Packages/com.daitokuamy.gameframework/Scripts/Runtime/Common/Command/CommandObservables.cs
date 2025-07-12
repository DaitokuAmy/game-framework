#if USE_R3
using System.Threading;
using R3;

#elif USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework {
    /// <summary>
    /// Command用のRx拡張メソッド
    /// </summary>
    public static class CommandObservables {
#if USE_R3
        /// <summary>
        /// CommandのStandby監視
        /// </summary>
        public static Observable<ICommand> StandbyedCommandEventAsObservable(this CommandManager source, CancellationToken ct = default) {
            return Observable.FromEvent<ICommand>(h => source.StandbyedCommandEvent += h, h => source.StandbyedCommandEvent -= h, ct);
        }

        /// <summary>
        /// CommandのExecuted監視
        /// </summary>
        public static Observable<ICommand> ExecutedCommandEventAsObservable(this CommandManager source, CancellationToken ct = default) {
            return Observable.FromEvent<ICommand>(h => source.ExecutedCommandEvent += h, h => source.ExecutedCommandEvent -= h, ct);
        }

        /// <summary>
        /// CommandのRemove監視
        /// </summary>
        public static Observable<ICommand> RemovedCommandEventAsObservable(this CommandManager source, CancellationToken ct = default) {
            return Observable.FromEvent<ICommand>(h => source.RemovedCommandEvent += h, h => source.RemovedCommandEvent -= h, ct);
        }
#elif USE_UNI_RX
        /// <summary>
        /// CommandのStandby監視
        /// </summary>
        public static IObservable<ICommand> StandbyedCommandEventAsObservable(this CommandManager source) {
            return Observable.FromEvent<ICommand>(h => source.StandbyedCommandEvent += h, h => source.StandbyedCommandEvent -= h);
        }

        /// <summary>
        /// CommandのExecuted監視
        /// </summary>
        public static IObservable<ICommand> ExecutedCommandEventAsObservable(this CommandManager source) {
            return Observable.FromEvent<ICommand>(h => source.ExecutedCommandEvent += h, h => source.ExecutedCommandEvent -= h);
        }

        /// <summary>
        /// CommandのRemove監視
        /// </summary>
        public static IObservable<ICommand> RemovedCommandEventAsObservable(this CommandManager source) {
            return Observable.FromEvent<ICommand>(h => source.RemovedCommandEvent += h, h => source.RemovedCommandEvent -= h);
        }
#endif
    }
}