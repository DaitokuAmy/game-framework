using System;
using R3;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// TitleOption
    /// </summary>
    public class BattleHudUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("メニューボタン")]
        private ButtonUIView _menuButtonView;
        
        /// <summary>戻るボタン押下通知</summary>
        public Observable<Unit> ClickedMenuButtonSubject => _menuButtonView.ClickedSubject;
    }
}