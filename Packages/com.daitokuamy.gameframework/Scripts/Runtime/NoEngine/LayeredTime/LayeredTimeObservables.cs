#if USE_R3
using R3;
#elif USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework.Core
{
    /// <summary>
    /// LayeredTime用のRx拡張メソッド
    /// </summary>
    public static class LayeredTimeObservables
    {
#if USE_R3
        /// <summary>
        /// TimeScaleの変更通知
        /// </summary>
        public static Observable<float> ChangedTimeScaleAsObservable(this LayeredTime source)
        {
            return Observable.FromEvent<float>(
                h => source.ChangedTimeScaleEvent += h,
                h => source.ChangedTimeScaleEvent -= h);
        }
#elif USE_UNI_RX
        /// <summary>
        /// TimeScaleの変更通知
        /// </summary>
        public static IObservable<float> ChangedTimeScaleAsObservable(this LayeredTime source)
        {
            return Observable.FromEvent<float>(
                h => source.ChangedTimeScaleEvent += h,
                h => source.ChangedTimeScaleEvent -= h);
        }
#endif
    }
}