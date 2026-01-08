using GameFramework.Core;

namespace SampleGame.Application {
    /// <summary>
    /// UITestのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface IAppNavigator {
        /// <summary>
        /// UITestへの遷移
        /// </summary>
        IProcess TransitionUITest();
    }
}