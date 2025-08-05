using GameFramework.UISystems;
using R3;
using ThirdPersonEngine;
using UnityEngine;

namespace SampleGame.Presentation.Introduction {
    /// <summary>
    /// TitleTop
    /// </summary>
    public class TitleTopUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("開始用ボタン")]
        private ButtonUIView _startButtonView;
        [SerializeField, Tooltip("オプションボタン")]
        private ButtonUIView _optionButtonView;
        [SerializeField, Tooltip("モデルビューアーボタン")]
        private ButtonUIView _modelViewerButtonView;
        [SerializeField, Tooltip("UITestボタン")]
        private ButtonUIView _uiTestButtonView;

        /// <summary>開始ボタン押下時通知</summary>
        public Observable<Unit> ClickedStartButtonSubject => _startButtonView.ClickedSubject;
        /// <summary>オプションボタン押下通知</summary>
        public Observable<Unit> ClickedOptionButtonSubject => _optionButtonView.ClickedSubject;
        /// <summary>モデルビューアーボタン押下通知</summary>
        public Observable<Unit> ClickedModelViewerButtonSubject => _modelViewerButtonView.ClickedSubject;
        /// <summary>UITestボタン押下通知</summary>
        public Observable<Unit> ClickedUITestButtonSubject => _uiTestButtonView.ClickedSubject;
    }
}