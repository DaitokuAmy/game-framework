using UnityEngine;

namespace ThirdPersonEngine.Editor {
    /// <summary>
    /// AssetToolWindowのModelPanel
    /// </summary>
    partial class AssetSetupToolWindow {
        /// <summary>
        /// Model用パネル
        /// </summary>
        private class ModelPanel : Panel {
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public ModelPanel(AssetSetupToolWindow window) : base(window) {
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                // 初期化ボタン
                if (GUILayout.Button("Setup")) {
                    AssetSetupTool.SetupSelectionModelAssets();
                }

                // VariantPrefab生成ボタン
                if (GUILayout.Button("Create Variant")) {
                    AssetSetupTool.CreateVariantPrefabs();
                }
            }
        }
    }
}