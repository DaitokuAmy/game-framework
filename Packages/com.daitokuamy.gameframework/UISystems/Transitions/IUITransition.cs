using System.Collections;

namespace GameFramework.UISystems {    
    /// <summary>
    /// UI遷移用インターフェース
    /// </summary>
    public interface IUiTransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="container">コンテナ</param>
        /// <param name="exitUIScreen">閉じるUIScreen</param>
        /// <param name="enterUIScreen">開くUIScreen</param>
        /// <param name="transitionType">遷移タイプ</param>
        /// <param name="immediate">即時遷移か</param>
        IEnumerator TransitRoutine(UIScreenContainer container, UIScreen exitUIScreen, UIScreen enterUIScreen, TransitionType transitionType, bool immediate);
    }
}