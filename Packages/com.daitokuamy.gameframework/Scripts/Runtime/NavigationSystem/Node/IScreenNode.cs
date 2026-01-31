using System.Collections;

namespace GameFramework.NavigationSystem {
    /// <summary>
    /// Screen単位のNavNodeインターフェース
    /// ※Session or Screen直下にのみ置けるNode
    /// </summary>
    public interface IScreenNode : INavNode {
        /// <summary>
        /// 開く前の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void PreOpen(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 開く処理
        /// ※immediateリクエストされた場合はコールされない
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator OpenRoutine(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 開いた後の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void PostOpen(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 閉じる前の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void PreClose(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 閉じる処理
        /// ※immediateリクエストされた場合はコールされない
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        IEnumerator CloseRoutine(TransitionHandle<INavNode> handle);

        /// <summary>
        /// 閉じた後の処理
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        void PostClose(TransitionHandle<INavNode> handle);
    }
}