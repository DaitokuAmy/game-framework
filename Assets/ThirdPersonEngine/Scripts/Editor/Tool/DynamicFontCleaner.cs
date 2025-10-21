using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ThirdPersonEngine.Editor {
    /// <summary>
    /// TextMeshProのDynamicFontを自動で戻すスクリプト
    /// </summary>
    [InitializeOnLoad]
    public static class DynamicFontCleaner {
        /// <summary>
        /// Staticコストラクタ
        /// </summary>
        static DynamicFontCleaner() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        /// <summary>
        /// プレイモードが変更された際の通知
        /// </summary>
        private static void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                RefreshFontAssets();
            }
        }

        /// <summary>
        /// シーン保存時のコールバック
        /// </summary>
        private static void OnSceneSaved(Scene scene) {
            RefreshFontAssets();
        }

        /// <summary>
        /// フォントアセットのリフレッシュ
        /// </summary>
        private static void RefreshFontAssets() {
            var tmpFontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var tmpFontAsset in tmpFontAssets) {
                if (tmpFontAsset != null && tmpFontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic) {
                    tmpFontAsset.ClearFontAssetData(true);
                    EditorUtility.SetDirty(tmpFontAsset);
                    Debug.Log($"DynamicFontCleaner: Clear font asset data. [{tmpFontAsset.name}]");
                }
            }

            AssetDatabase.Refresh();
        }
    }
}