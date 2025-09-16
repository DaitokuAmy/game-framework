using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using SampleGame.Application;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// OutGame関連のSituationService処理
    /// </summary>
    partial class SituationService {
        /// <inheritdoc/>
        IProcess ISituationService.TransitionSortieTop() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<SortieTopSituation>(transitionType: transitionType);
        }

        /// <inheritdoc/>
        IProcess ISituationService.TransitionSortieRoleSelect() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<SortieRoleSelectSituation>(transitionType: transitionType);
        }

        /// <inheritdoc/>
        IProcess ISituationService.TransitionSortieRoleInformation() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<SortieRoleInformationSituation>(transitionType: transitionType);
        }

        /// <inheritdoc/>
        IProcess ISituationService.TransitionSortieMissionSelect() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }
            
            return Transition<SortieMissionSelectSituation>(transitionType: transitionType);
        }

        /// <inheritdoc/>
        IProcess ISituationService.TransitionSortieDifficultySelect() {
            var transitionType = TransitionType.ScreenCross;
            if (!CheckParentSituation<OutGameSceneSituation>()) {
                transitionType = TransitionType.SceneDefault;
            }

            return Transition<SortieDifficultySelectSituation>(transitionType: transitionType);
        }

        /// <summary>
        /// OutGame関連のSituationの初期化
        /// </summary>
        private void SetupOutGameSituations(Situation parentSituation) {
            var outGameSceneSituation = new OutGameSceneSituation();
            outGameSceneSituation.SetParent(parentSituation);

            // 出撃画面
            var sortieSituation = new SortieSituation();
            sortieSituation.SetParent(outGameSceneSituation);
            var sortieTopSituation = new SortieTopSituation();
            sortieTopSituation.SetParent(sortieSituation);
            var sortieRoleSelectSituation = new SortieRoleSelectSituation();
            sortieRoleSelectSituation.SetParent(sortieSituation);
            var sortieRoleInformationSituation = new SortieRoleInformationSituation();
            sortieRoleInformationSituation.SetParent(sortieRoleSelectSituation);
            var sortieMissionSelectSituation = new SortieMissionSelectSituation();
            sortieMissionSelectSituation.SetParent(sortieSituation);
            var sortieDifficultySelectSituation = new SortieDifficultySelectSituation();
            sortieDifficultySelectSituation.SetParent(sortieMissionSelectSituation);
        }

        /// <summary>
        /// OutGame関連のTreeNode初期化
        /// </summary>
        private StateTreeNode<Type> SetupOutGameTreeNodes(StateTreeNode<Type> parentNode) {
            // 出撃画面
            var sortieTopNode = ConnectNode<SortieTopSituation>(parentNode);
            var roleSelectNode = ConnectNode<SortieRoleSelectSituation>(sortieTopNode);
            var roleInformationNode = ConnectNode<SortieRoleInformationSituation>(roleSelectNode);
            var missionSelectNode = ConnectNode<SortieMissionSelectSituation>(sortieTopNode);
            var difficultySelectNode = ConnectNode<SortieDifficultySelectSituation>(missionSelectNode);
            return sortieTopNode;
        }
    }
}