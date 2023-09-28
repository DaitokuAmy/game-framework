#if USE_UNI_RX
using System;
using UniRx;

namespace GameFramework.CommandSystems {
    /// <summary>
    /// Command用のRx拡張メソッド
    /// </summary>
    public static class CommandObservables {
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
    }
}
#endif