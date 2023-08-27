using System.Threading;
#if USE_UNI_TASK
using System.Runtime.ExceptionServices;
using Cysharp.Threading.Tasks;
#endif

namespace GameFramework.Core {
    /// <summary>
    /// IProcess用の拡張メソッド
    /// </summary>
    public static class ProcessExtensions {
#if USE_UNI_TASK
        /// <summary>
        /// IProcessをUniTaskに変換
        /// </summary>
        public static async UniTask ToUniTask(this IProcess source, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default) {
            if (!source.IsDone) {
                await UniTask.WaitUntil(() => source.IsDone, timing, cancellationToken);
            }

            if (source.Exception != null) {
                ExceptionDispatchInfo.Capture(source.Exception).Throw();
            }
        }

        /// <summary>
        /// IProcessをUniTaskに変換
        /// </summary>
        public static async UniTask<T> ToUniTask<T>(this IProcess<T> source,
            PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default) {
            if (!source.IsDone) {
                await UniTask.WaitUntil(() => source.IsDone, timing, cancellationToken);
            }

            if (source.Exception != null) {
                ExceptionDispatchInfo.Capture(source.Exception).Throw();
            }

            return source.Result;
        }
#endif
    }
}