using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEditor;
using UnityEngine;

namespace SampleGame.VfxViewer.Editor {
    /// <summary>
    /// VfxViewerのSettingsパネル
    /// </summary>
    partial class VfxViewerWindow {
        /// <summary>
        /// RecordingPanel
        /// </summary>
        private class RecordingPanel : PanelBase {
            public override string Title => "Recording";
            
            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
            }
            
            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                var recordingModel = VfxViewerModel.Get().RecordingModel;
                
                // オプション
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var options = recordingModel.Options;
                    options = (RecordingOptions)EditorGUILayout.EnumFlagsField("Options", options);
                    if (scope.changed) {
                        recordingModel.SetOptions(options);
                    }
                }
                
                // ターン時間
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var duration = recordingModel.RotationDuration;
                    duration = EditorGUILayout.FloatField("Rotation Duration", duration);
                    if (scope.changed) {
                        recordingModel.SetRotationDuration(duration);
                    }
                }
                
                // 録画開始
                var recordingController = Services.Get<RecordingController>();
                using (new EditorGUI.DisabledScope(recordingController.IsRecording)) {
                    if (GUILayout.Button("Record")) {
                        recordingController.RecordAsync()
                            .ToUniTask()
                            .Forget();
                    }
                }
            }
        }
    }
}