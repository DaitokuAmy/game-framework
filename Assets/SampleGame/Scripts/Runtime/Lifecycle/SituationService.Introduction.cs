using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Introduction関連のSituationService処理
    /// </summary>
    partial class AppNavigator {
        /// <inheritdoc/>
        IProcess IAppNavigator.TransitionTitleTop() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<IntroductionSessionNode>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<TitleTopScreenNode>(transitionType: transitionType);
        }
        
        /// <inheritdoc/>
        IProcess IAppNavigator.TransitionTitleOption() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<IntroductionSessionNode>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<TitleOptionScreenNode>(transitionType: transitionType);
        }
        
        /// <summary>
        /// Introduction関連のSituationの初期化
        /// </summary>
        private void SetupIntroductionSituations(Situation parentSituation) {
            var introductionSceneSituation = new IntroductionSessionNode();
            introductionSceneSituation.SetParent(parentSituation);
            var titleTopSituation = new TitleTopScreenNode();
            titleTopSituation.SetParent(introductionSceneSituation);
            var titleOptionSituation = new TitleOptionScreenNode();
            titleOptionSituation.SetParent(introductionSceneSituation);
        }

        /// <summary>
        /// Introduction関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupIntroductionTreeNodes(StateTreeNode<Type> parentNode) {
            var titleTopNode = ConnectNode<TitleTopScreenNode>(parentNode);
            var titleOptionNode = ConnectNode<TitleOptionScreenNode>(titleTopNode);
            return titleTopNode;
        }
    }
}