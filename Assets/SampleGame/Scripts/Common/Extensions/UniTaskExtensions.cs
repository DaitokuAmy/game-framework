using Cysharp.Threading.Tasks;
using SampleGame.Domain.Battle;

namespace SampleGame {
    /// <summary>
    /// UniTask拡張
    /// </summary>
    public static class UniTaskExtensions {
        /// <summary>
        /// HandleをUniTaskに追加する
        /// </summary>
        public static UniTask AddHandle(this UniTask source, ActionHandle handle) {
            return source.SuppressCancellationThrow().ContinueWith(isCanceled => {
                if (isCanceled) {
                    handle.Cancel();
                }
                else {
                    handle.Complete();
                }
            });
        }
    }
}