using System;
using UniRx;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// カットシーンの情報関連をまとめたスクリーン
    /// </summary>
    public class CutsceneInformationUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("スキップボタン")]
        private ButtonUIView _skipButtonView;
        
        /// <summary>スキップボタン押下通知</summary>
        public IObservable<Unit> OnClickedSkipSubject => _skipButtonView.ClickedSubject;
    }
}