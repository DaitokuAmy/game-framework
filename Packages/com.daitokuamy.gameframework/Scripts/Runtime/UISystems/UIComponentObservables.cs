#if USE_R3
using R3;
#endif

#if USE_UNI_RX
using System;
using UniRx;
#endif

namespace GameFramework.UISystems {
    /// <summary>
    /// UIComponent用のRx拡張メソッド
    /// </summary>
    public static class UIComponentObservables {
#if USE_R3
        /// <summary>
        /// RecyclableScrollListのView生成通知
        /// </summary>
        public static Observable<RecyclableScrollList.IItemView> CreatedItemViewAsObservable(this RecyclableScrollList self) {
            return Observable.FromEvent<RecyclableScrollList.IItemView>(h => self.CreatedItemViewEvent += h, h => self.CreatedItemViewEvent -= h);
        }
        
        /// <summary>
        /// RecyclableScrollListのView削除通知
        /// </summary>
        public static Observable<RecyclableScrollList.IItemView> DeletedItemViewAsObservable(this RecyclableScrollList self) {
            return Observable.FromEvent<RecyclableScrollList.IItemView>(h => self.DeletedItemViewEvent += h, h => self.DeletedItemViewEvent -= h);
        }
#endif
#if USE_UNI_RX
        /// <summary>
        /// RecyclableScrollListのView生成通知
        /// </summary>
        public static IObservable<RecyclableScrollList.IItemView> CreatedItemViewAsObservable(this RecyclableScrollList self) {
            return Observable.FromEvent<RecyclableScrollList.IItemView>(h => self.CreatedItemViewEvent += h, h => self.CreatedItemViewEvent -= h);
        }
        
        /// <summary>
        /// RecyclableScrollListのView削除通知
        /// </summary>
        public static IObservable<RecyclableScrollList.IItemView> DeletedItemViewAsObservable(this RecyclableScrollList self) {
            return Observable.FromEvent<RecyclableScrollList.IItemView>(h => self.DeletedItemViewEvent += h, h => self.DeletedItemViewEvent -= h);
        }
#endif
    }
}