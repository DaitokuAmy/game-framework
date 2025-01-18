using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation.Introduction {
    /// <summary>
    /// Introduction用のUIService
    /// </summary>
    public class IntroductionUIService : UIService {
        [SerializeField, Tooltip("タイトルトップ用スクリーン")]
        private TitleTopUIScreen _titleTopScreen;
        [SerializeField, Tooltip("タイトルオプション用スクリーン")]
        private TitleOptionUIScreen _titleOptionScreen;

        /// <summary>タイトルトップ操作用</summary>
        public TitleTopUIScreen TitleTopUIScreen => _titleTopScreen;
        /// <summary>タイトルオプション操作用</summary>
        public TitleOptionUIScreen TitleOptionUIScreen => _titleOptionScreen;
    }
}