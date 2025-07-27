using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation.UITest {
    /// <summary>
    /// UITestHud用のUIService
    /// </summary>
    public class UITestHudUIService : UIService {
        [SerializeField, Tooltip("HUD用スクリーン")]
        private UITestHudUIScreen _uiTestHudScreen;

        /// <summary>HudScreen操作用</summary>
        public UITestHudUIScreen UITestHudUIScreen => _uiTestHudScreen;
    }
}