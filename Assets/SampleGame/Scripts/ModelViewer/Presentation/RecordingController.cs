using System;
using System.Collections;
using System.IO;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using GameFramework.TaskSystems;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
#endif

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 録画するためのコントローラ
    /// </summary>
    public class RecordingController : TaskBehaviour {
        [SerializeField, Tooltip("録画用のフレームレート")]
        private int _frameRate = 60;
        [SerializeField, Tooltip("録画解像度")]
        private int _resolutionWidth = 1920;
        [SerializeField, Tooltip("録画解像度")]
        private int _resolutionHeight = 1080;
        [SerializeField, Tooltip("出力先フォルダ")]
        private string _outputPath = "ModelViewerRecordings";

#if UNITY_EDITOR
        private CoroutineRunner _coroutineRunner;

        private RecorderControllerSettings _settings;
        private MovieRecorderSettings _movieRecorderSettings;
#endif

        /// <summary>録画中か</summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// 録画処理
        /// </summary>
        public AsyncOperationHandle RecordAsync() {
#if UNITY_EDITOR
            if (IsRecording) {
                return new AsyncOperationHandle();
            }

            // 録画開始
            IsRecording = true;

            var op = new AsyncOperator();
            var viewerModel = ModelViewerModel.Get();
            var recordingModel = viewerModel.RecordingModel;
            var setupDataId = viewerModel.PreviewActorModel.SetupDataId;
            _coroutineRunner.StartCoroutine(
                RecordRoutine(setupDataId, recordingModel.RotationDuration, recordingModel.Options),
                () => {
                    op.Completed();
                    IsRecording = false;
                },
                () => {
                    op.Aborted();
                    IsRecording = false;
                },
                ex => {
                    op.Aborted(ex);
                    IsRecording = false;
                });
            return op;
#else
            return new AsyncOperationHandle();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void AwakeInternal() {
            _settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _settings.SetRecordModeToManual();
            _movieRecorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            _movieRecorderSettings.AudioInputSettings.PreserveAudio = false;
            _settings.AddRecorderSettings(_movieRecorderSettings);
            _coroutineRunner = new CoroutineRunner();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void OnDestroyInternal() {
            _coroutineRunner.Dispose();

            Destroy(_movieRecorderSettings);
            Destroy(_settings);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            _coroutineRunner.Update();
        }

        /// <summary>
        /// 録画用の設定を初期化
        /// </summary>
        private void SetupRecordingSettings(string fileKey) {
            _settings.FrameRate = _frameRate;
            _movieRecorderSettings.ImageInputSettings = new GameViewInputSettings {
                OutputWidth = _resolutionWidth,
                OutputHeight = _resolutionHeight,
                RecordTransparency = false,
                FlipFinalOutput = false
            };
            _movieRecorderSettings.OutputFile = Path.Combine(_outputPath, fileKey);
            _movieRecorderSettings.Enabled = true;
            _settings.AddRecorderSettings(_movieRecorderSettings);
        }

        /// <summary>
        /// 録画ルーチン
        /// </summary>
        private IEnumerator RecordRoutine(string setupDataId, float rotationDuration, RecordingOptions flags) {
            var slot = Services.Get<ModelViewerSettings>().PreviewSlot;
            var cameraComponent = Services.Get<CameraManager>().GetCameraComponent<PreviewCameraComponent>("Default");
            var dirLight = Services.Get<EnvironmentManager>().CurrentLight;

            void SetSlotAngle(float angle) {
                if (slot == null) {
                    return;
                }

                var slotEulerAngles = slot.eulerAngles;
                slotEulerAngles.y = angle;
                slot.eulerAngles = slotEulerAngles;
            }

            void SetCameraAngle(float angle) {
                if (cameraComponent == null) {
                    return;
                }

                cameraComponent.AngleY = Mathf.Repeat(angle + 180.0f, 360.0f);
            }

            void SetLightAngle(float angle) {
                if (dirLight == null) {
                    return;
                }

                var lightTrans = dirLight.transform;
                var lightEulerAngles = lightTrans.eulerAngles;
                lightEulerAngles.y = angle;
                lightTrans.eulerAngles = lightEulerAngles;
            }

            // 録画用設定を初期化
            SetupRecordingSettings(setupDataId);

            var recorderController = new RecorderController(_settings);

            // 各種向きをリセット
            SetSlotAngle(0.0f);
            SetCameraAngle(0.0f);
            SetLightAngle(0.0f);

            IEnumerator RotateRoutine(Action<float> setAngleAction) {
                var time = 0.0f;
                while (true) {
                    // 回転
                    var angle = 360.0f * Mathf.Clamp01(time / rotationDuration);
                    setAngleAction(angle);
                    if (time >= rotationDuration) {
                        break;
                    }

                    time += Time.deltaTime;
                    yield return null;
                }
            }

            // 録画開始
            recorderController.PrepareRecording();
            recorderController.StartRecording();

            // 回転の実行
            if ((flags & RecordingOptions.ActorRotation) != 0) {
                yield return RotateRoutine(SetSlotAngle);
            }

            if ((flags & RecordingOptions.CameraRotation) != 0) {
                yield return RotateRoutine(SetCameraAngle);
            }

            if ((flags & RecordingOptions.LightRotation) != 0) {
                yield return RotateRoutine(SetLightAngle);
            }

            // 録画停止
            recorderController.StopRecording();
        }
#endif
    }
}