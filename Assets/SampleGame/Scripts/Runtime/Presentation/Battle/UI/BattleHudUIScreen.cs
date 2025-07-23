using GameFramework.UISystems;
using R3;
using ThirdPersonEngine;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// TitleOption
    /// </summary>
    public class BattleHudUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("メニューボタン")]
        private ButtonUIView _menuButtonView;
        [SerializeField, Tooltip("テスト用のスクロールリスト")]
        private RecyclableScrollList _testScrollList;
        
        /// <summary>テスト用のスクロールリスト</summary>
        public RecyclableScrollList TestScrollList => _testScrollList;
        
        /// <summary>戻るボタン押下通知</summary>
        public Observable<Unit> ClickedMenuButtonSubject => _menuButtonView.ClickedSubject;
    }
}