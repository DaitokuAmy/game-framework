using System.Linq;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityDebugMenu;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// メインシステム（Debug用処理）
    /// </summary>
    partial class MainSystem {
        /// <summary>
        /// デバッグ機能の初期化
        /// </summary>
        private void SetupDebug() {
            DebugMenu.AddWindowItem("Common/Config", _ => {
                var qualityLabels = Enumerable.Range(0, QualitySettings.count).Select(x => QualitySettings.names[x]).ToArray();
                using (var changeCheckScope = new DebugMenuUtil.ChangeCheckScope()) {
                    var index = DebugMenuUtil.ArrowOrderField("Quality", QualitySettings.GetQualityLevel(), qualityLabels);
                    if (changeCheckScope.Changed) {
                        QualitySettings.SetQualityLevel(index);
                    }
                }
            });

            DebugMenu.AddWindowItem("Common/UI", _ => {
                using (var changeCheckScope = new DebugMenuUtil.ChangeCheckScope()) {
                    if (DebugMenuUtil.ButtonField("Canvas Toggle", "実行")) {
                        var uiManager = Services.Get<UIManager>();
                        var canvases = uiManager.GetCanvases();
                        if (canvases.Length > 0) {
                            var active = canvases[0].gameObject.activeSelf;
                            foreach (var canvas in canvases) {
                                canvas.gameObject.SetActive(!active);
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// デバッグ機能の解放
        /// </summary>
        private void CleanupDebug() {
            DebugMenu.RemoveItem("Common");
        }
    }
}