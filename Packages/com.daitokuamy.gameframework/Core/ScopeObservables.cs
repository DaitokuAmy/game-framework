#if USE_UNI_RX

using System;
using UniRx;

namespace GameFramework.Core {
    /// <summary>
    /// IScope用のRx拡張メソッド
    /// </summary>
    public static class ScopeObservables {
        /// <summary>
        /// IDisposableのScope登録
        /// </summary>
        public static IObservable<T> TakeUntil<T>(this IObservable<T> self, IScope scope) {
            return self.TakeUntil(Observable.FromEvent(h => scope.OnExpired += h, h => scope.OnExpired -= h));
        }
    }
}

#endif // USE_UNI_RX