using System.Linq;
using GameFramework.UISystem;
using R3;
using SampleGameEngine;
using UnityEngine;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面のミッション選択部分
    /// </summary>
    public class SortieMissionSelectUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("項目のButtonViewリスト")]
        private ButtonUIView[] _buttonViews;

        /// <summary>選択通知</summary>
        public Observable<int> SelectedIndexSubject => _buttonViews
            .Select((x, i) => x.ClickedSubject.Select(_ => i))
            .Merge();
    }
}