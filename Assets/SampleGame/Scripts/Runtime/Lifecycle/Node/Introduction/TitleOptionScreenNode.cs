using System.Collections;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Introduction;
using R3;
using SampleGame.Application;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// TitleOption用のScreenNode
    /// </summary>
    public class TitleOptionScreenNode : ScreenNode {
        [Inject]
        private UIManager _uiManager;
        [Inject]
        private IAppNavigator _appNavigator;

        /// <inheritdoc/>
        protected override IEnumerator OpenRoutine(TransitionHandle<INavNode> handle, IScope animationScope) {
            yield return base.OpenRoutine(handle, animationScope);

            var introductionUIService = _uiManager.GetService<IntroductionUIService>();
            yield return introductionUIService.TitleOptionUIScreen.OpenAsync();
        }

        /// <inheritdoc/>
        protected override void PostOpen(TransitionHandle<INavNode> handle) {
            base.PostOpen(handle);

            var introductionUIService = _uiManager.GetService<IntroductionUIService>();
            introductionUIService.TitleOptionUIScreen.OpenAsync(immediate: true);
        }

        /// <inheritdoc/>
        protected override IEnumerator CloseRoutine(TransitionHandle<INavNode> handle, IScope animationScope) {
            yield return base.CloseRoutine(handle, animationScope);

            var introductionUIService = _uiManager.GetService<IntroductionUIService>();
            yield return introductionUIService.TitleOptionUIScreen.CloseAsync();
        }

        /// <inheritdoc/>
        protected override void PostClose(TransitionHandle<INavNode> handle) {
            base.PostClose(handle);

            var introductionUIService = _uiManager.GetService<IntroductionUIService>();
            introductionUIService.TitleOptionUIScreen.CloseAsync(immediate: true);
        }

        /// <inheritdoc/>
        protected override void Activate(TransitionHandle<INavNode> handle, IScope scope) {
            base.Activate(handle, scope);

            var introductionUIService = _uiManager.GetService<IntroductionUIService>();

            // 戻るボタン
            introductionUIService.TitleOptionUIScreen.ClickedBackButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.Back(cross: true);
                });
        }
    }
}