using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 常駐なUIService
    /// </summary>
    public class ResidentUIService : UIService {
        [SerializeField, Tooltip("ローディング画面用のコンテナ")]
        private UISheetContainer _loadingScreenContainer;
        [SerializeField, Tooltip("ブロック画面")]
        private BlockUIScreen _blockScreen;
        [SerializeField, Tooltip("通知用スクリーン")]
        private NotificationUIScreen _notificationScreen;
        [SerializeField, Tooltip("フェーダー用スクリーン")]
        private FaderUIScreen _faderScreen;

        /// <summary>画面ブロック用のスクリーン</summary>
        public BlockUIScreen BlockScreen => _blockScreen;
        /// <summary>通知用のスクリーン</summary>
        public NotificationUIScreen NotificationScreen => _notificationScreen;
        /// <summary>フェーダー用のスクリーン</summary>
        public FaderUIScreen FaderScreen => _faderScreen;

        /// <summary>
        /// ローディングの表示
        /// </summary>
        public IProcess<UIScreen> ShowLoading(string key) {
            return _loadingScreenContainer.Change(key);
        }

        /// <summary>
        /// ローディングの非表示
        /// </summary>
        public IProcess HideLoading() {
            return _loadingScreenContainer.Clear();
        }
    }
}
