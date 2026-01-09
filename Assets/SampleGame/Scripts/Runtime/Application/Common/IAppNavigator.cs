using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;

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
        TransitionHandle<INavNode> TransitionTo(Type nodeType, bool reset = false, Action<INavNode> setupAction = null);
    }
}
