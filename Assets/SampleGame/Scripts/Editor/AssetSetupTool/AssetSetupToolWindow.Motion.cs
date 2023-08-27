using UnityEngine;

namespace SampleGame.Editor {
    /// <summary>
    /// AssetToolWindowのModelPanel
    /// </summary>
    partial class AssetSetupToolWindow {
        /// <summary>
        /// Motion用パネル
        /// </summary>
        private class MotionPanel : Panel {
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public MotionPanel(AssetSetupToolWindow window) : base(window) {
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                // 初期化ボタン
                if (GUILayout.Button("Setup")) {
                    AssetSetupTool.SetupSelectionMotionAssets();
                }
            }
        }
    }
}