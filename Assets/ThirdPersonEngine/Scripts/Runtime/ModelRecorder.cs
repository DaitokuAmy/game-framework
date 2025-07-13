using System;
using System.Collections;
using System.IO;
using GameFramework;
using GameFramework.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
#endif

namespace ThirdPersonEngine {
    /// <summary>
    /// モデル録画用クラス
    /// </summary>
    public class ModelRecorder : MonoBehaviour {
        /// <summary>
        /// 録画設定
        /// </summary>
        public interface ISettings {
            /// <summary>録画オプションマスク</summary>
            Options Options { get; }
            /// <summary>回転時間</summary>
            float RotationDuration { get; }
        }

        /// <summary>
        /// 録画オプションマスク
        /// </summary>
        [Flags]
        public enum Options {
            ActorRotation = 1 << 0,
            CameraRotation = 1 << 1,
            LightRotation = 1 << 2,
        }

        /// <summary>
        /// 録画出力フォーマット
        /// </summary>
        public enum OutputFormat {
            Mp4,
            WebM,
            Mov,
        }

        [SerializeField, Tooltip("録画用のフレームレート")]
        private int _frameRate = 60;
        [SerializeField, Tooltip("録画解像度")]
        private int _resolutionWidth = 1920;
        [SerializeField, Tooltip("録画解像度")]
        private int _resolutionHeight = 1080;
        [SerializeField, Tooltip("出力先フォルダ")]
        private string _outputPath = "ModelViewerRecordings";
        [SerializeField, Tooltip("アクターを回す際のSlot")]
        private Transform actorSlot;
        [SerializeField, Tooltip("ライトを回す際のSlot")]
        private Transform _lightSlot;
        [SerializeField, Tooltip("カメラを回す際のSlot")]
        private Transform _cameraSlot;

#if UNITY_EDITOR
        private CoroutineRunner _coroutineRunner;

        private RecorderControllerSettings _settings;
        private MovieRecorderSettings _movieRecorderSettings;
#endif

        /// <summary>録画中か</summary>
        public bool IsRecording { get; private set; }
        /// <summary>モデルを回す際のSlot</summary>
        public Transform ActorSlot {
            get => actorSlot;
            set => actorSlot = value;
        }
        /// <summary>ライトを回す際のSlot</summary>
        public Transform LightSlot {
            get => _lightSlot;
            set => _lightSlot = value;
        }
        /// <summary>カメラを回す際のSlot</summary>
        public Transform CameraSlot {
            get => _cameraSlot;
            set => _cameraSlot = value;
        }

        /// <summary>
        /// 録画処理
        /// </summary>
        public AsyncOperationHandle Record(string displayName, ISettings settings) {
#if UNITY_EDITOR
            if (IsRecording) {
                return new AsyncOperationHandle();
            }

            // 録画開始
            IsRecording = true;

            var op = new AsyncOperator();
            _coroutineRunner.StartCoroutine(
                RecordRoutine(displayName, settings.RotationDuration, settings.Options),
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
            return AsyncOperationHandle.CompletedHandle;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
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
        private void OnDestroy() {
            _coroutineRunner.Dispose();

            Destroy(_movieRecorderSettings);
            Destroy(_settings);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
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
        private IEnumerator RecordRoutine(string assetKey, float rotationDuration, Options options) {
            void SetSlotAngle(Transform slot, float angle) {
                if (slot == null) {
                    return;
                }

                var slotEulerAngles = slot.eulerAngles;
                slotEulerAngles.y = angle;
                slot.eulerAngles = slotEulerAngles;
            }

            // 録画用設定を初期化
            SetupRecordingSettings(assetKey);

            var recorderController = new RecorderController(_settings);

            // 各種向きをリセット
            SetSlotAngle(ActorSlot, 0.0f);
            SetSlotAngle(CameraSlot, 0.0f);
            SetSlotAngle(LightSlot, 0.0f);

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
            if ((options & Options.ActorRotation) != 0) {
                yield return RotateRoutine(angle => SetSlotAngle(ActorSlot, angle));
            }

            if ((options & Options.CameraRotation) != 0) {
                yield return RotateRoutine(angle => SetSlotAngle(CameraSlot, angle));
            }

            if ((options & Options.LightRotation) != 0) {
                yield return RotateRoutine(angle => SetSlotAngle(LightSlot, angle));
            }

            // 録画停止
            recorderController.StopRecording();
        }
#endif
    }
}