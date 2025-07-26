using System.Collections;

namespace GameFramework {
    /// <summary>
    /// 遷移処理用インターフェース
    /// </summary>
    public interface ITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="resolver">遷移処理解決者</param>
        /// <param name="immediate">即時遷移か</param>
        IEnumerator TransitionRoutine(ITransitionResolver resolver, bool immediate = false);
    }
}