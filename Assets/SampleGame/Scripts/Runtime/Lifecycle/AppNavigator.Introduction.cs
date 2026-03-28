using Cysharp.Threading.Tasks;
using GameFramework.NavigationSystem;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Introduction関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToTitleTop() {
            var (transition, effects) = GetDefaultTransitionInfo<IntroductionSessionNode>();
            await _engine.TransitionTo(Id.TitleTop, transition, effects);
        }
        
        /// <inheritdoc/>
        async UniTask IAppNavigator.TransitionToTitleOption() {
            var (transition, effects) = GetDefaultTransitionInfo<IntroductionSessionNode>();
            await _engine.TransitionTo(Id.TitleOption, transition, effects);
        }

        /// <summary>
        /// IntroductionのLifecycle構築
        /// </summary>
        private void SetupIntroductionLifecycle(SessionNodeBuilder introduction) {
            introduction.AddScreen<TitleTopScreenNode>(Id.TitleTop)
                .AddScreen<TitleOptionScreenNode>(Id.TitleOption);
        }

        /// <summary>
        /// IntroductionのTitleTop以降の遷移ツリー
        /// </summary>
        /// <param name="titleTop"></param>
        private void ConnectIntroductionTitleTopTreeNode(NavNodeTreeRouterNodeBuilder titleTop) {
            titleTop.Connect(Id.TitleOption)
                .Connect(Id.SortieTop, ConnectOutGameSortieTopTreeNode)
                .SetGlobalShortcut();
        }
    }
}