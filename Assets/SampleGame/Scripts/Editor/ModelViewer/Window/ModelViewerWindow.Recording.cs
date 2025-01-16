using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using SampleGame.Presentation.ModelViewer;
using UnityEditor;
using UnityEngine;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// ModelViewerのSettingsパネル
    /// </summary>
    partial class ModelViewerWindow {
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
                var appService = Services.Get<ModelViewerAppService>();
                var recordingModel = appService.DomainService.RecordingModel;
                
                // オプション
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var options = recordingModel.Options;
                    options = (RecordingOptions)EditorGUILayout.EnumFlagsField("Options", options);
                    if (scope.changed) {
                        //appService.SetRecordingOptions(options);
                    }
                }
                
                // ターン時間
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var duration = recordingModel.RotationDuration;
                    duration = EditorGUILayout.FloatField("Rotation Duration", duration);
                    if (scope.changed) {
                        //appService.SetRecordingRotationDuration(duration);
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