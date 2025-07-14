using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// BattleOverlay用のUIService
    /// </summary>
    public class BattleOverlayUIService : UIService {
        [SerializeField, Tooltip("オーバーレイ演出用のコンテナー")]
        private UISheetContainer _overlayScreenContainer;

        /// <summary>
        /// 勝利画面表示
        /// </summary>
        public void PlayWin() {
            _overlayScreenContainer.Change("Win");
        }
        
        /// <summary>
        /// 敗北画面表示
        /// </summary>
        public void PlayLose() {
            _overlayScreenContainer.Change("Lose");
        }
        
        /// <summary>
        /// 画面をクリア
        /// </summary>
        public void Clear() {
            _overlayScreenContainer.Clear();
        }
    }
}