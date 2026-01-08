using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using GameFramework.UISystems;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Introduction用のSessionNode
    /// </summary>
    public class IntroductionSessionNode : SceneSessionNode {
        [Inject]
        private UIManager _uiManager;
        
        /// <inheritdoc/>
        protected override string ScenePath => "Assets/SampleGame/Scenes/introduction.unity";

        /// <inheritdoc/>
        protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield return base.LoadRoutine(handle, scope);
            
            // UI読み込み
            var tasks = new List<UniTask>();
            tasks.Add(LoadUIAsync(scope, scope.Token));

            yield return UniTask.WhenAll(tasks).ToCoroutine();
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            UniTask LoadAsync(string assetKey) {
                return _uiManager.LoadSceneAsync(assetKey).RegisterTo(unloadScope).ToUniTask(cancellationToken: ct);
            }
            
            return UniTask.WhenAll(LoadAsync("introduction"));
        }
    }
}