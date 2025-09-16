using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Introduction関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <inheritdoc/>
        IProcess ISituationService.TransitionTitleTop() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<IntroductionSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<TitleTopSituation>(transitionType: transitionType);
        }
        
        /// <inheritdoc/>
        IProcess ISituationService.TransitionTitleOption() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<IntroductionSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<TitleOptionSituation>(transitionType: transitionType);
        }
        
        /// <summary>
        /// Introduction関連のSituationの初期化
        /// </summary>
        private void SetupIntroductionSituations(Situation parentSituation) {
            var introductionSceneSituation = new IntroductionSceneSituation();
            introductionSceneSituation.SetParent(parentSituation);
            var titleTopSituation = new TitleTopSituation();
            titleTopSituation.SetParent(introductionSceneSituation);
            var titleOptionSituation = new TitleOptionSituation();
            titleOptionSituation.SetParent(introductionSceneSituation);
        }

        /// <summary>
        /// Introduction関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupIntroductionTreeNodes(StateTreeNode<Type> parentNode) {
            var titleTopNode = ConnectNode<TitleTopSituation>(parentNode);
            var titleOptionNode = ConnectNode<TitleOptionSituation>(titleTopNode);
            return titleTopNode;
        }
    }
}