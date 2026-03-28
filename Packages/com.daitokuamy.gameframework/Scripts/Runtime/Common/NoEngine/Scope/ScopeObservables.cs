#if USE_R3
using R3;
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
            return self.TakeUntil(Observable.FromEvent(h => scope.ExpiredEvent += h, h => scope.ExpiredEvent -= h));
        }
#endif
    }
}