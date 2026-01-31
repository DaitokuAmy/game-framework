using GameFramework;
using GameFramework;
using SampleGame.Application;
using VContainer;
using R3;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面難易度選択用のPresenter
    /// </summary>
    public class SortieDifficultySelectPresenter : UIScreenLogic<SortieDifficultySelectUIScreen> {
        [Inject]
        private IAppNavigator _appNavigator;

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.SelectedIndexSubject
                .TakeUntil(scope)
                .Subscribe(idx => {
                    switch (idx) {
                        default:
                            _appNavigator.TransitionToBattle();
                            break;
                    }
                });
        }
    }
}