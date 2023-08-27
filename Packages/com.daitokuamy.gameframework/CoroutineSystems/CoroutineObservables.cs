#if USE_UNI_RX

using System;
using System.Collections;
using UniRx;

namespace GameFramework.CoroutineSystems {
    /// <summary>
    /// コルーチンのRx拡張
    /// </summary>
    public static class CoroutineObservables {
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
    }
}

#endif // USE_UNI_RX