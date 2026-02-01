using System.Collections;
using UnityEngine.SceneManagement;

namespace GameFramework.NavigationSystem {
    /// <summary>
    /// シーン切り替えSessionNode基底
    /// </summary>
    public abstract class SceneSessionNode : SessionNode {
        /// <summary>読み込み前の空シーンPath</summary>
        protected virtual string EmptyScenePath => "empty";
        /// <summary>読み込むシーンPath</summary>
        protected abstract string ScenePath { get; }

        /// <inheritdoc/>
        protected override ITransition OverrideTransition(INavNode nextNode, ITransition transition) {
            if (transition is not OutInTransition) {
                return new OutInTransition();
            }

            return transition;
        }

        /// <inheritdoc/>
        protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            if (!string.IsNullOrEmpty(EmptyScenePath)) {
                yield return SceneManager.LoadSceneAsync(EmptyScenePath, LoadSceneMode.Single);
            }

            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
        }

        /// <inheritdoc/>
        protected override void Unload(TransitionHandle<INavNode> handle) {
            if (!string.IsNullOrEmpty(EmptyScenePath)) {
                SceneManager.LoadScene(EmptyScenePath, LoadSceneMode.Single);
            }
        }
    }
}