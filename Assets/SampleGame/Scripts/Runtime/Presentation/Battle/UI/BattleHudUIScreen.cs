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
        private RecyclableScrollList _testScrollVList;
        [SerializeField, Tooltip("テスト用のスクロールリスト")]
        private RecyclableScrollList _testScrollHList;
        [SerializeField, Tooltip("テスト用のスクロールスナッパー")]
        private ScrollSnapper _scrollSnapperV;
        [SerializeField, Tooltip("テスト用のスクロールスナッパー")]
        private ScrollSnapper _scrollSnapperH;
        
        /// <summary>テスト用のスクロールリスト(垂直)</summary>
        public RecyclableScrollList TestScrollVList => _testScrollVList;
        /// <summary>テスト用のスクロールリスト(水平)</summary>
        public RecyclableScrollList TestScrollHList => _testScrollHList;
        
        /// <summary>戻るボタン押下通知</summary>
        public Observable<Unit> ClickedMenuButtonSubject => _menuButtonView.ClickedSubject;

        public void SetScrollSnapperIndex(int index, bool immediate = false) {
            _scrollSnapperV.SetTargetIndex(index, immediate);
            _scrollSnapperH.SetTargetIndex(index, immediate);
        }
    }
}