using System.Collections;
using GameFramework.Core;
using UnityEngine.SceneManagement;

namespace GameFramework.NavigationSystems {
    /// <summary>
    /// 加算Scene付きSessionNode基底
    /// </summary>
    public abstract class AdditiveSceneSessionNode : SessionNode {
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
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Additive);
        }

        /// <inheritdoc/>
        protected override void Unload(TransitionHandle<INavNode> handle) {
            SceneManager.UnloadSceneAsync(ScenePath);
        }
    }
}