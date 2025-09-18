using Cysharp.Threading.Tasks;
using GameFramework.CameraSystems;
using GameFramework.Core;
using SampleGame.Application.ModelViewer;
using ThirdPersonEngine;
using Unity.Cinemachine;
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
            public override string Label => "Recording";

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void DrawGuiInternal(ModelViewerWindow window) {
                var appService = window.Resolver.Resolve<ModelViewerAppService>();
                var recordingModel = appService.DomainService.RecordingModel;

                // オプション
                // using (var scope = new EditorGUI.ChangeCheckScope()) {
                //     var options = recordingModel.Options;
                //     options = (ModelRecorder.Options)EditorGUILayout.EnumFlagsField("Options", options);
                //     if (scope.changed) {
                //         appService.SetRecordingOptions(options);
                //     }
                // }

                // ターン時間
                // using (var scope = new EditorGUI.ChangeCheckScope()) {
                //     var duration = recordingModel.RotationDuration;
                //     duration = EditorGUILayout.FloatField("Rotation Duration", duration);
                //     if (scope.changed) {
                //         appService.SetRecordingRotationDuration(duration);
                //     }
                // }

                // 録画開始
                var recorder = window.Resolver.Resolve<ModelRecorder>();
                using (new EditorGUI.DisabledScope(recorder.IsRecording)) {
                    if (GUILayout.Button("Record")) {
                        var displayName = appService.DomainService.PreviewActorModel.Master.Name;
                        var settings = appService.DomainService.RecordingModel;

                        // カメラの情報を複製
                        var cameraManager = window.Resolver.Resolve<CameraManager>();
                        var defaultCameraComponent = cameraManager.GetCameraComponent<PreviewCameraComponent>("Default");
                        var relativePosition = defaultCameraComponent.CalcRelativePosition();
                        var lookAtPosition = defaultCameraComponent.CalcLookAtPosition();

                        var cameraComponent = cameraManager.GetCameraComponent<DefaultCameraComponent>("Capture");
                        var captureFollow = cameraManager.GetTargetPoint("CaptureFollow");
                        var captureLookAt = cameraManager.GetTargetPoint("CaptureLookAt");
                        if (cameraComponent.VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) is CinemachineFollow follow) {
                            follow.FollowOffset = relativePosition;
                        }

                        if (cameraComponent.VirtualCamera is CinemachineCamera cinemachineCamera) {
                            cinemachineCamera.Lens.FieldOfView = defaultCameraComponent.Fov;
                        }

                        captureFollow.position = lookAtPosition;
                        captureLookAt.position = lookAtPosition;

                        // カメラをOnにして録画
                        cameraManager.ForceActivate("Capture");

                        // recorder.Record(displayName, settings).ToUniTask()
                        //     .ContinueWith(() => {
                        //         // カメラを戻す
                        //         cameraManager.ForceDeactivate("Capture");
                        //     });
                    }
                }
            }
        }
    }
}