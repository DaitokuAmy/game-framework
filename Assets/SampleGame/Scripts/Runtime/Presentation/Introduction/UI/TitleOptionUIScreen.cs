using System;
using R3;
using UnityEngine;

namespace SampleGame.Presentation.Introduction {
    /// <summary>
    /// TitleOption
    /// </summary>
    public class TitleOptionUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("戻るボタン")]
        private ButtonUIView _backButtonView;
        
        /// <summary>戻るボタン押下通知</summary>
        public Observable<Unit> ClickedBackButtonSubject => _backButtonView.ClickedSubject;
    }
}