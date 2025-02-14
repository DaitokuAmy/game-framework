using System.Collections;
using System;

namespace GameFramework.UISystems {
    /// <summary>
    /// CrossするUiTransition
    /// </summary>
    public class CrossUITransition : IUITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="container">コンテナ</param>
        /// <param name="exitUIScreen">閉じるUIScreen</param>
        /// <param name="enterUIScreen">開くUIScreen</param>
        /// <param name="transitionType">遷移タイプ</param>
        /// <param name="immediate">即時遷移か</param>
        /// <param name="initAction">初期化用アクション</param>
        IEnumerator IUITransition.TransitRoutine(UIScreenContainer container, UIScreen exitUIScreen, UIScreen enterUIScreen, TransitionType transitionType, bool immediate, Action<UIScreen> initAction) {
            var exitHandle = default(AnimationHandle);
            var enterHandle = default(AnimationHandle);
            
            if (exitUIScreen != null) {
                exitHandle = exitUIScreen.CloseAsync(transitionType, immediate);
            }

            if (enterUIScreen != null) {
                initAction?.Invoke(enterUIScreen);
                enterHandle = enterUIScreen.OpenAsync(transitionType, immediate);
            }

            while (!exitHandle.IsDone || !enterHandle.IsDone) {
                yield return null;
            }
        }
    }
}