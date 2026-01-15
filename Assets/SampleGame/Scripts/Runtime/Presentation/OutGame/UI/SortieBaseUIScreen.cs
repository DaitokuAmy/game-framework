using GameFramework.UISystems;
using R3;
using ThirdPersonEngine;
using UnityEngine;

namespace SampleGame.Presentation.OutGame {
    /// <summary>
    /// 出撃画面のベーススクリーン
    /// </summary>
    public class SortieBaseUIScreen : AnimatableUIScreen {
        [SerializeField, Tooltip("戻るボタン")]
        private ButtonUIView _backButtonView;
        
        /// <summary>戻るボタン押下通知</summary>
        public Observable<Unit> ClickedBackButtonSubject => _backButtonView.ClickedSubject;

        /// <summary>
        /// 戻るボタンの表示状態設定
        /// </summary>
        public void SetVisibleBackButton(bool visible) {
            _backButtonView.gameObject.SetActive(visible);
        }
    }
}