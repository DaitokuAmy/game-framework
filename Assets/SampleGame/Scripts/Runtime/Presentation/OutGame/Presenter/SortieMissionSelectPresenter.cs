using GameFramework;
using GameFramework.Core;
using SampleGame.Application;
using VContainer;
using R3;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面ミッション選択用のPresenter
    /// </summary>
    public class SortieMissionSelectPresenter : UIScreenLogic<SortieMissionSelectUIScreen> {
        [Inject]
        private IAppNavigator _appNavigator;

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.SelectedIndexSubject
                .TakeUntil(scope)
                .Subscribe(idx => {
                    switch (idx) {
                        default:
                            _appNavigator.TransitionToSortieDifficultySelect();
                            break;
                    }
                });
        }
    }
}