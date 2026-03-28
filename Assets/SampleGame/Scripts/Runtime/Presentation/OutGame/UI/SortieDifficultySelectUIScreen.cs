using System.Linq;
using GameFramework.UISystem;
using R3;
using SampleGameEngine;
using UnityEngine;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面の難易度選択選択部分
    /// </summary>
    public class SortieDifficultySelectUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("閉じるボタンView")]
        private ButtonUIView _closeButtonView;
        
        [SerializeField, Tooltip("項目のButtonViewリスト")]
        private ButtonUIView[] _buttonViews;

        /// <summary>選択通知</summary>
        public Observable<int> SelectedIndexSubject => _buttonViews
            .Select((x, i) => x.ClickedSubject.Select(_ => i))
            .Merge();
        
        /// <summary>閉じる通知</summary>
        public Observable<Unit> ClickedCloseButtonSubject => _closeButtonView.ClickedSubject;
    }
}