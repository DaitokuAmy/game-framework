using System.Linq;
using GameFramework.UISystems;
using R3;
using ThirdPersonEngine;
using UnityEngine;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面のTop部分
    /// </summary>
    public class SortieTopUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("項目のButtonViewリスト")]
        private ButtonUIView[] _buttonViews;

        /// <summary>選択通知</summary>
        public Observable<int> SelectedIndexSubject => _buttonViews
            .Select((x, i) => x.ClickedSubject.Select(_ => i))
            .Merge();
    }
}