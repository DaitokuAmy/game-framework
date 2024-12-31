using System.Collections;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using GameFramework.SituationSystems;
using GameFramework.UISystems;

namespace SampleGame.Introduction {
    /// <summary>
    /// Title用のSceneSituation
    /// </summary>
    public class IntroductionSceneSituation : SceneSituation {
        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/introduction.unity";
        protected override string EmptySceneAssetPath => "Assets/SampleGame/Scenes/empty.unity";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
            
            // UI読み込み
            yield return LoadUIRoutine(scope);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private IEnumerator LoadUIRoutine(IScope scope) {
            var uiManager = Services.Get<UIManager>();
            yield return new MergedCoroutine(
                uiManager.LoadSceneAsync("ui_title").ScopeTo(scope)
            );
        }
    }
}