using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// BattleHud用のUIService
    /// </summary>
    public class BattleHudUIService : UIService {
        [SerializeField, Tooltip("バトルHUD用スクリーン")]
        private BattleHudUIScreen _battleHudScreen;

        /// <summary>バトルHUD操作用</summary>
        public BattleHudUIScreen BattleHudUIScreen => _battleHudScreen;
    }
}