using System;
using System.Collections;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;
using GameFramework.Core;

#if USE_R3
using R3;
#endif
#if USE_UNI_RX
using UniRx;
#endif
#if USE_UNI_TASK
using Cysharp.Threading.Tasks;
#endif

namespace GameFramework {
    /// <summary>
    /// コルーチンの拡張
    /// </summary>
    public static class CoroutineExtensions {
        // WaitForSeconds.m_Seconds
        private static readonly FieldInfo WaitForSecondsAccessor = typeof(WaitForSeconds).GetField("m_Seconds",
            BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

        /// <summary>
        /// WaitForSecondsのIEnumerator変換
        /// </summary>
        public static IEnumerator GetEnumerator(this WaitForSeconds source) {
            var second = (float)WaitForSecondsAccessor.GetValue(source);
            var startTime = DateTimeOffset.UtcNow;
            while (true) {
                yield return null;
                var elapsed = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
                if (elapsed >= second) {
                    break;
                }
            }
        }

#if USE_R3
        /// <summary>
        /// UniRxのStreamをCoroutineRunnerに対応したCoroutineに変換して実行する
        /// </summary>
        /// <param name="source">Coroutine化するStream</param>
        /// <param name="scope">Streamのキャンセルスコープ</param>
        /// <param name="onNext">Streamの戻り値確保用</param>
        /// <param name="onError">Streamのエラー確保用</param>
        /// <param name="throwException">エラー時にExceptionを吐き出すか</param>
        public static IEnumerator StartAsEnumerator<T>(this Observable<T> source, IScope scope, Action<T> onNext = null, Action<Exception> onError = null, bool throwException = true) {
            // Rx実行
            var done = false;
            var exception = default(Exception);
            source.Subscribe(x => { onNext?.Invoke(x); },
                    ex => {
                        onError?.Invoke(ex);
                        exception = ex;
                        done = true;
                    },
                    x => { done = true; }
                )
                .RegisterTo(scope);

            // 完了待ち
            while (!done && exception == null) {
                yield return null;
            }

            // エラーが発生していたら再スロー
            if (exception != null && throwException) {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }
#endif

#if USE_UNI_RX
        /// <summary>
        /// UniRxのStreamをCoroutineRunnerに対応したCoroutineに変換して実行する
        /// </summary>
        /// <param name="source">Coroutine化するStream</param>
        /// <param name="scope">Streamのキャンセルスコープ</param>
        /// <param name="onNext">Streamの戻り値確保用</param>
        /// <param name="onError">Streamのエラー確保用</param>
        /// <param name="throwException">エラー時にExceptionを吐き出すか</param>
        public static IEnumerator StartAsEnumerator<T>(this IObservable<T> source, IScope scope, Action<T> onNext = null,
            Action<Exception> onError = null, bool throwException = true) {
            // Rx実行
            var done = false;
            var exception = default(Exception);
            source.Subscribe(x => { onNext?.Invoke(x); },
                    ex => {
                        onError?.Invoke(ex);
                        exception = ex;
                        done = true;
                    },
                    () => { done = true; }
                )
                .RegisterTo(scope);

            // 完了待ち
            while (!done && exception == null) {
                yield return null;
            }

            // エラーが発生していたら再スロー
            if (exception != null && throwException) {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }
#endif

#if USE_UNI_TASK
        /// <summary>
        /// コルーチン実行（UniTask）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="enumerator"></param>
        /// <param name="cancellationToken"></param>
        public static UniTask StartCoroutineAsync(this CoroutineRunner source, IEnumerator enumerator,
            CancellationToken cancellationToken) {
            var completionSource = new UniTaskCompletionSource();
            if (cancellationToken.IsCancellationRequested) {
                completionSource.TrySetCanceled(cancellationToken);
                return completionSource.Task;
            }

            var registration = cancellationToken.Register(() => completionSource.TrySetCanceled());

            source.StartCoroutine(enumerator, () => { completionSource.TrySetResult(); },
                () => {
                    completionSource.TrySetCanceled();
                    registration.Dispose();
                },
                exception => {
                    completionSource.TrySetException(exception);
                    registration.Dispose();
                }, cancellationToken);

            return completionSource.Task;
        }
#endif
    }
}