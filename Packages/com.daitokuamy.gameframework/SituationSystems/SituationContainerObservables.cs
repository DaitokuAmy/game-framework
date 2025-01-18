#if USE_R3
using System;
using R3;

#elif USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework.SituationSystems {
    /// <summary>
    /// IProcess用のRx拡張メソッド
    /// </summary>
    public static class SituationContainerObservables {
#if USE_R3
        /// <summary>
        /// ChangedCurrentEventのR3変換
        /// </summary>
        public static Observable<Situation> ChangedCurrentAsObservable(this SituationContainer self) {
            return Observable.FromEvent<Situation>(h => self.ChangedCurrentEvent += h, h => self.ChangedCurrentEvent -= h);
        }
#elif USE_UNI_RX
        /// <summary>
        /// ChangedCurrentEventのR3変換
        /// </summary>
        public static IObservable<Situation> ChangedCurrentAsObservable(this SituationContainer self) {
            return Observable.FromEvent<Situation>(h => self.ChangedCurrentEvent += h, h => self.ChangedCurrentEvent -= h);
        }
#endif
    }
}