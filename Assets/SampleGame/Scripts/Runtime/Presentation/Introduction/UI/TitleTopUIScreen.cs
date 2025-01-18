using System;
using UniRx;
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

        /// <summary>開始ボタン押下時通知</summary>
        public IObservable<Unit> ClickedStartButtonSubject => _startButtonView.ClickedSubject;
        /// <summary>オプションボタン押下通知</summary>
        public IObservable<Unit> ClickedOptionButtonSubject => _optionButtonView.ClickedSubject;
        /// <summary>モデルビューアーボタン押下通知</summary>
        public IObservable<Unit> ClickedModelViewerButtonSubject => _modelViewerButtonView.ClickedSubject;
    }
}