#if USE_R3
using System.Threading;
using R3;
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
#endif
    }
}
