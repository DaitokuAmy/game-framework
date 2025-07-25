using GameFramework.Core;

#if USE_R3
using R3;
#endif

#if USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework {
    /// <summary>
    /// IScope用のRx拡張メソッド
    /// </summary>
    public static class ScopeObservables {
#if USE_R3
        /// <summary>
        /// IDisposableのScope登録
        /// </summary>
        public static Observable<T> TakeUntil<T>(this Observable<T> self, IScope scope) {
            return self.TakeUntil(R3.Observable.FromEvent(h => scope.ExpiredEvent += h, h => scope.ExpiredEvent -= h));
        }
#endif
#if USE_UNI_RX
        /// <summary>
        /// IDisposableのScope登録
        /// </summary>
        public static IObservable<T> TakeUntil<T>(this IObservable<T> self, IScope scope) {
            return self.TakeUntil(UniRx.Observable.FromEvent(h => scope.ExpiredEvent += h, h => scope.ExpiredEvent -= h));
        }
#endif
    }
}