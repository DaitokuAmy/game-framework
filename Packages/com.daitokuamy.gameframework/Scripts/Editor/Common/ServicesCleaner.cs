using GameFramework.Core;
using UnityEditor;

namespace GameFramework.Editor {
    /// <summary>
    /// Servicesの中身をクリアするためのエディタ拡張
    /// </summary>
    public static class ServicesCleaner {
        /// <summary>
        /// エディタ起動時の処理
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnInitializeOnLoad() {
            // Play/Edit切り替わり時にインスタンスを解放
            EditorApplication.playModeStateChanged += change => {
                switch (change) {
                    case PlayModeStateChange.EnteredEditMode:
                    case PlayModeStateChange.ExitingEditMode:
                        Services.Cleanup();
                        break;
                }
            };
        }
    }
}