using System;
using System.Collections;
#if USE_R3
using R3;

#elif USE_UNI_RX
using UniRx;
#endif

namespace GameFramework {
    /// <summary>
    /// コルーチンのRx拡張
    /// </summary>
    public static class CoroutineObservables {
#if USE_R3
        /// <summary>
        /// コルーチンの開始処理
        /// </summary>
        public static Observable<Unit> StartCoroutineAsync(this CoroutineRunner source, IEnumerator enumerator, Action onCanceled = null) {
            return Observable.Create<Unit>(observer => {
                var isDone = false;
                var coroutine = source.StartCoroutine(enumerator, () => {
                    isDone = true;
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }, () => {
                    isDone = true;
                    onCanceled?.Invoke();
                    observer.OnCompleted();
                }, exception => {
                    isDone = true;
                    observer.OnErrorResume(exception);
                });

                return Disposable.Create(() => {
                    if (!isDone) {
                        // ストリームキャンセルによるコルーチン停止
                        source.StopCoroutine(coroutine);
                    }
                });
            });
        }
#elif USE_UNI_RX
        /// <summary>
        /// コルーチンの開始処理
        /// </summary>
        public static IObservable<Unit> StartCoroutineAsync(this CoroutineRunner source, IEnumerator enumerator,
            Action onCanceled = null) {
            return Observable.Create<Unit>(observer => {
                var isDone = false;
                var coroutine = source.StartCoroutine(enumerator, () => {
                    isDone = true;
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }, () => {
                    isDone = true;
                    onCanceled?.Invoke();
                    observer.OnCompleted();
                }, exception => {
                    isDone = true;
                    observer.OnError(exception);
                });

                return Disposable.Create(() => {
                    if (!isDone) {
                        // ストリームキャンセルによるコルーチン停止
                        source.StopCoroutine(coroutine);
                    }
                });
            });
        }
#endif
    }
}