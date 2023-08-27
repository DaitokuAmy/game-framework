using GameFramework.Core;
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
            public override string Title => "Settings";
            
            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
            }
            
            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                var settings = ModelViewerModel.Get().SettingsModel;

                // カメラ制御タイプ
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var cameraControlType = settings.CameraControlType.Value;
                    cameraControlType = (CameraControlType)EditorGUILayout.EnumPopup("Camera Control Type", cameraControlType);
                    if (scope.changed) {
                        settings.ChangeCameraControlType(cameraControlType);
                    }
                }
                
                // タイムスケール
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var timeScale = settings.LayeredTime.LocalTimeScale;
                    timeScale = EditorGUILayout.Slider("Time Scale", timeScale, 0.0f, SettingsModel.TimeScaleMax);
                    if (scope.changed) {
                        settings.SetTimeScale(timeScale);
                    }
                }
            }
        }
    }
}