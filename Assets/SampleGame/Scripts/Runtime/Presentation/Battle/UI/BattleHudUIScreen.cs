using System;
using UniRx;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// TitleOption
    /// </summary>
    public class BattleHudUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("メニューボタン")]
        private ButtonUIView _menuButtonView;
        
        /// <summary>戻るボタン押下通知</summary>
        public IObservable<Unit> ClickedMenuButtonSubject => _menuButtonView.ClickedSubject;
    }
}