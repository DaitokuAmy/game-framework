using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// BattleOverlay用のUIService
    /// </summary>
    public class BattleOverlayUIService : UIService {
        [SerializeField, Tooltip("オーバーレイ演出用のコンテナー")]
        private UIScreenContainer _overlayScreenContainer;
        
        /// <summary>オーバーレイ演出用のコンテナー</summary>
        public UIScreenContainer OverlayScreenContainer => _overlayScreenContainer;
    }
}