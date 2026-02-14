using GameFramework;
using GameFramework.TweenSystem;
using SampleGame.Application;
using R3;
using SampleGame.Presentation.Introduction;
using UnityEngine;
using VContainer;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// タイトルトップ用のPresenter
    /// </summary>
    public class TitleTopPresenter : UIScreenLogic<TitleTopUIScreen> {
        [Inject]
        private IAppNavigator _appNavigator;
        
        private TweenPlayer _tweenPlayer = new();

        /// <inheritdoc/>
        protected override void ActivateInternal(IScope scope) {
            Screen.ClickedStartButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.TransitionToSortieTop();
                });
            Screen.ClickedOptionButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.TransitionToTitleOption();
                });
            Screen.ClickedUITestButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.TransitionToUITest();
                });
            Screen.ClickedModelViewerButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _appNavigator.TransitionToModelViewer();
                });
        }
        
        /// <inheritdoc/>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            if (Input.GetKeyDown(KeyCode.Space)) {
                var icon = Screen.TestIcon;
                var target = Screen.Points[Random.Range(0, Screen.Points.Length)];
                var moveTo = _tweenPlayer.MoveTo(icon, target.position, 0.5f, Space.World)
                    .SetEase(EaseType.EaseInCubic)
                    .OnComplete(() => Debug.Log("Completed_Space"));
                _tweenPlayer.ForceCompleteAll();
                _tweenPlayer.Play(moveTo);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1)) {
                var icon = Screen.TestIcon;
                var target = Screen.Points[Random.Range(0, Screen.Points.Length)];
                var sequence = _tweenPlayer.CreateSequence()
                    .Append(_tweenPlayer.MoveTo(icon, target.position, 0.5f))
                    .Append(_tweenPlayer.MoveTo(icon, Vector3.zero, 0.5f))
                    .OnComplete(() => Debug.Log("Completed_0"));;
                _tweenPlayer.ForceCompleteAll();
                _tweenPlayer.Play(sequence);
            }
            
            _tweenPlayer.Tick(Time.deltaTime);
        }
    }
}