using GameFramework.Core;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using UnityEditor;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// ModelViewerのSettingsパネル
    /// </summary>
    partial class ModelViewerWindow {
        /// <summary>
        /// SettingsPanel
        /// </summary>
        private class SettingsPanel : PanelBase {
            public override string Label => "Settings";
            
            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void DrawGuiInternal(ModelViewerWindow window) {
                var appService = window.Resolver.Resolve<ModelViewerAppService>();
                var settingsModel = appService.DomainService.SettingsModel;

                // カメラ制御タイプ
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var cameraControlType = settingsModel.CameraControlType;
                    cameraControlType = (CameraControlType)EditorGUILayout.EnumPopup("Camera Control Type", cameraControlType);
                    if (scope.changed) {
                        appService.ChangeCameraControlType(cameraControlType);
                    }
                }
                
                // タイムスケール
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var timeScale = settingsModel.LayeredTime.LocalTimeScale;
                    timeScale = EditorGUILayout.Slider("Time Scale", timeScale, 0.0f, SettingsModel.TimeScaleMax);
                    if (scope.changed) {
                        appService.SetTimeScale(timeScale);
                    }
                }
            }
        }
    }
}