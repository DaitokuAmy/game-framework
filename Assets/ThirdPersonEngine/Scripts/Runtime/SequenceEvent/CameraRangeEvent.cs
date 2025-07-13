using System;
using ActionSequencer;
using GameFramework.CameraSystems;
using Unity.Cinemachine;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 汎用カメラ切り替え用のイベント
    /// </summary>
    public class CameraRangeEvent : RangeSequenceEvent {
        // ブレンド情報
        [Serializable]
        public struct Blend {
            [Tooltip("カスタムブレンドを使うか")]
            public bool active;
            [Tooltip("ブレンドパラメータ")]
            public CinemachineBlendDefinition blendDefinition;
        }

        [Tooltip("グループキー")]
        public string groupKey = "";
        [Tooltip("遷移に使うカメラ名")]
        public string cameraName = "";
        [Tooltip("入り用のブレンド設定")]
        public Blend toBlend;
        [Tooltip("抜け用のブレンド設定")]
        public Blend fromBlend;
    }

    /// <summary>
    /// CameraRangeEvent用のハンドラ
    /// </summary>
    public class CameraRangeEventHandler : RangeSequenceEventHandler<CameraRangeEvent> {
        private CameraManager _cameraManager;
        private Func<string, bool> _checkFunc;
        private bool _activateCamera;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraManager">操作対象のCameraManager</param>
        /// <param name="checkFunc">適用するかのチェックフラグ</param>
        public void Setup(CameraManager cameraManager, Func<string, bool> checkFunc = null) {
            _cameraManager = cameraManager;
            _checkFunc = checkFunc;
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void OnEnter(CameraRangeEvent sequenceEvent) {
            _activateCamera = false;

            if (_checkFunc?.Invoke(sequenceEvent.cameraName) ?? true) {
                if (sequenceEvent.toBlend.active) {
                    _cameraManager.Activate(sequenceEvent.groupKey, sequenceEvent.cameraName, sequenceEvent.toBlend.blendDefinition);
                }
                else {
                    _cameraManager.Activate(sequenceEvent.groupKey, sequenceEvent.cameraName);
                }

                _activateCamera = true;
            }
        }

        /// <summary>
        /// 終了時処理
        /// </summary>
        protected override void OnExit(CameraRangeEvent sequenceEvent) {
            if (_activateCamera) {
                if (sequenceEvent.fromBlend.active) {
                    _cameraManager.Deactivate(sequenceEvent.groupKey, sequenceEvent.cameraName, sequenceEvent.fromBlend.blendDefinition);
                }
                else {
                    _cameraManager.Deactivate(sequenceEvent.groupKey, sequenceEvent.cameraName);
                }
            }
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void OnCancel(CameraRangeEvent sequenceEvent) => OnExit(sequenceEvent);
    }
}