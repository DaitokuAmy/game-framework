using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.UITest;
using ThirdPersonEngine;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// UITest用のSessionNode
    /// </summary>
    public class UITestSessionNode : SceneSessionNode {
        [Inject]
        private UIManager _uiManager;
        
        /// <inheritdoc/>
        protected override string ScenePath => "Assets/SampleGame/Scenes/ui_test.unity";

        /// <inheritdoc/>
        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);
            
            SetupInfrastructures(builder);
            SetupManagers(builder);
            SetupDomains(builder);
            SetupApplications(builder);
            SetupFactories(builder);
        }

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
            
            SetupPresentations(scope);
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            UniTask LoadAsync(string assetKey) {
                return _uiManager.LoadSceneAsync(assetKey).RegisterTo(unloadScope).ToUniTask(cancellationToken: ct);
            }

            return UniTask.WhenAll(LoadAsync("ui_test"));
        }

        /// <summary>
        /// Infrastructure初期化
        /// </summary>
        private void SetupInfrastructures(IContainerBuilder builder) {
        }

        /// <summary>
        /// Manager初期化
        /// </summary>
        private void SetupManagers(IContainerBuilder builder) {
        }

        /// <summary>
        /// Domain初期化
        /// </summary>
        private void SetupDomains(IContainerBuilder builder) {
        }

        /// <summary>
        /// Application初期化
        /// </summary>
        private void SetupApplications(IContainerBuilder builder) {
        }

        /// <summary>
        /// Factory初期化
        /// </summary>
        private void SetupFactories(IContainerBuilder builder) {
        }

        /// <summary>
        /// Presentation初期化
        /// </summary>
        private void SetupPresentations(IScope scope) {
            var hudUIService = _uiManager.GetService<UITestHudUIService>();
            var dialogUIService = _uiManager.GetService<UITestDialogUIService>();
            hudUIService.UITestHudUIScreen.RegisterHandler(LogicUtility.SetupLogic(new HudUIScreenPresenter(), UpdateOrder.Presenter, ObjectResolver, false, scope));
            dialogUIService.SetBuyItemDialogHandler(() => new BuyItemUIDialogPresenter().RegisterTo(scope));
        }
    }
}