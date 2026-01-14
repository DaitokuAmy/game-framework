using Cysharp.Threading.Tasks;
using GameFramework.Core;

namespace SampleGame.Application {
    /// <summary>
    /// IntroductionのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface IAppNavigator {
        /// <summary>
        /// タイトルトップへの遷移
        /// </summary>
        UniTask TransitionToTitleTop();
        
        /// <summary>
        /// タイトルオプションへの遷移
        /// </summary>
        UniTask TransitionToTitleOption();
    }
}
