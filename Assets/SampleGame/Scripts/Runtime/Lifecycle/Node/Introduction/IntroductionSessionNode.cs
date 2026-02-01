using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.NavigationSystem;
using GameFramework.UISystem;
using SampleGame.Presentation.Introduction;
using SampleGame.Presentation.OutGame;
using SampleGameEngine;
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

        /// <inheritdoc/>
        protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield return base.InitializeRoutine(handle, scope);
            
            // Presenter初期化
            SetupPresentations(scope);
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

        /// <summary>
        /// Presentation初期化
        /// </summary>
        private void SetupPresentations(IScope scope) {
            var uiService = _uiManager.GetService<IntroductionUIService>();
            uiService.TitleTopUIScreen.RegisterHandler(LogicUtility.CreateLogic<TitleTopPresenter>(ObjectResolver, false, scope));
            uiService.TitleOptionUIScreen.RegisterHandler(LogicUtility.CreateLogic<TitleOptionPresenter>(ObjectResolver, false, scope));
        }
    }
}