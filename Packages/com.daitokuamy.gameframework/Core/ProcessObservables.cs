#if USE_UNI_RX

using System;
using UniRx;

namespace GameFramework.Core {
    /// <summary>
    /// IProcess用のRx拡張メソッド
    /// </summary>
    public static class ProcessObservables {
        /// <summary>
        /// IProcessのRx変換
        /// </summary>
        public static IObservable<Unit> AsObservable(this IProcess self) {
            return Observable.Create<Unit>(observer => {
                if (self.IsDone) {
                    if (self.Exception != null) {
                        observer.OnError(self.Exception);
                    }
                    else {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    }

                    return Disposable.Empty;
                }

                return Observable.EveryUpdate()
                    .Subscribe(_ => {
                        if (!self.IsDone) {
                            return;
                        }

                        if (self.Exception != null) {
                            observer.OnError(self.Exception);
                        }
                        else {
                            observer.OnNext(Unit.Default);
                            observer.OnCompleted();
                        }
                    });
            });
        }

        /// <summary>
        /// IProcessのRx変換
        /// </summary>
        public static IObservable<T> AsObservable<T>(this IProcess<T> self) {
            return Observable.Create<T>(observer => {
                if (self.IsDone) {
                    if (self.Exception != null) {
                        observer.OnError(self.Exception);
                    }
                    else {
                        observer.OnNext(self.Result);
                        observer.OnCompleted();
                    }

                    return Disposable.Empty;
                }

                return Observable.EveryUpdate()
                    .Subscribe(_ => {
                        if (!self.IsDone) {
                            return;
                        }

                        if (self.Exception != null) {
                            observer.OnError(self.Exception);
                        }
                        else {
                            observer.OnNext(self.Result);
                            observer.OnCompleted();
                        }
                    });
            });
        }
    }
}

#endif // USE_UNI_RX