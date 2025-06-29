using System.Collections;
using System.Threading;
#if USE_UNI_TASK
using Cysharp.Threading.Tasks;
#endif

namespace GameFramework {
    /// <summary>
    /// LayeredTime用の拡張メソッド
    /// </summary>
    public static class LayeredTimeExtensions {
        /// <summary>
        /// LayeredTime指定で一定時間待つIEnumerator拡張
        /// </summary>
        public static IEnumerator WaitForSeconds(this LayeredTime source, float duration) {
            var timer = duration;
            while (true) {
                timer -= source.DeltaTime;
                if (timer <= 0) {
                    yield break;
                }

                yield return null;
            }
        }

#if USE_UNI_TASK
        /// <summary>
        /// LayeredTime指定で一定時間待つUniTask拡張
        /// </summary>
        public static UniTask WaitForSeconds(this LayeredTime source, float duration, CancellationToken token) {
            var timer = duration;
            return UniTask.WaitUntil(() => {
                timer -= source.DeltaTime;
                return timer <= 0.0f;
            }, cancellationToken: token);
        }
#endif
    }
}