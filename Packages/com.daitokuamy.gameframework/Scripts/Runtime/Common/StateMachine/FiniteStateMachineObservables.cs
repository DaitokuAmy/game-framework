#if USE_R3
using System;
using R3;
#elif USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework
{
    /// <summary>
    /// FiniteStateMachine用のRx拡張メソッド
    /// </summary>
    public static class FiniteStateMachineObservables
    {
#if USE_R3
        /// <summary>
        /// Stateの変更通知
        /// </summary>
        public static Observable<TKey> ChangedStateAsObservable<TState, TKey>(this FiniteStateMachine<TState, TKey> source)
            where TState : IState<TKey>
            where TKey : IComparable
        {
            return Observable.FromEvent<TKey>(
                h => source.ChangedStateEvent += h,
                h => source.ChangedStateEvent -= h);
        }
#elif USE_UNI_RX
        /// <summary>
        /// Stateの変更通知
        /// </summary>
        public static IObservable<TKey> ChangedStateAsObservable<TState, TKey>(this FiniteStateMachine<TState, TKey> source)
            where TState : IState<TKey>
            where TKey : IComparable
        {
            return Observable.FromEvent<TKey>(
                h => source.ChangedStateEvent += h,
                h => source.ChangedStateEvent -= h);
        }
#endif
    }
}