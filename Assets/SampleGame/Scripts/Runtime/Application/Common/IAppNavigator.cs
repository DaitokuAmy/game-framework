using System;
using GameFramework;
using GameFramework.NavigationSystem;

namespace SampleGame.Application {
    /// <summary>
    /// Situation遷移に関するサービスインターフェース
    /// </summary>
    public partial interface IAppNavigator {
        /// <summary>
        /// 戻る処理
        /// </summary>
        TransitionHandle<INavNode> Back(int depth = 1);

        /// <summary>
        /// 汎用遷移処理
        /// </summary>
        TransitionHandle<INavNode> TransitionTo(int nodeId, bool refresh, Action<ScreenNode> setupAction);
    }
}
