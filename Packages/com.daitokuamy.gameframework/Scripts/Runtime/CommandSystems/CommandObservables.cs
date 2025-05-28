#if USE_R3
using System.Threading;
using R3;

#elif USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework.CommandSystems {
    /// <summary>
    /// Command用のRx拡張メソッド
    /// </summary>
    public static class CommandObservables {
#if USE_R3
        /// <summary>
        /// CommandのStandby監視
        /// </summary>
        public static Observable<ICommand> OnStandbyedCommandAsObservable(this CommandManager source, CancellationToken ct = default) {
            return Observable.FromEvent<ICommand>(h => source.OnStandbyedCommandAction += h, h => source.OnStandbyedCommandAction -= h, ct);
        }

        /// <summary>
        /// CommandのExecuted監視
        /// </summary>
        public static Observable<ICommand> OnExecutedCommandAsObservable(this CommandManager source, CancellationToken ct = default) {
            return Observable.FromEvent<ICommand>(h => source.OnExecutedCommandAction += h, h => source.OnExecutedCommandAction -= h, ct);
        }

        /// <summary>
        /// CommandのRemove監視
        /// </summary>
        public static Observable<ICommand> OnRemovedCommandAsObservable(this CommandManager source, CancellationToken ct = default) {
            return Observable.FromEvent<ICommand>(h => source.OnRemovedCommandAction += h, h => source.OnRemovedCommandAction -= h, ct);
        }
#elif USE_UNI_RX
        /// <summary>
        /// CommandのStandby監視
        /// </summary>
        public static IObservable<ICommand> OnStandbyedCommandAsObservable(this CommandManager source) {
            return Observable.FromEvent<ICommand>(h => source.OnStandbyedCommandAction += h, h => source.OnStandbyedCommandAction -= h);
        }

        /// <summary>
        /// CommandのExecuted監視
        /// </summary>
        public static IObservable<ICommand> OnExecutedCommandAsObservable(this CommandManager source) {
            return Observable.FromEvent<ICommand>(h => source.OnExecutedCommandAction += h, h => source.OnExecutedCommandAction -= h);
        }

        /// <summary>
        /// CommandのRemove監視
        /// </summary>
        public static IObservable<ICommand> OnRemovedCommandAsObservable(this CommandManager source) {
            return Observable.FromEvent<ICommand>(h => source.OnRemovedCommandAction += h, h => source.OnRemovedCommandAction -= h);
        }
#endif
    }
}