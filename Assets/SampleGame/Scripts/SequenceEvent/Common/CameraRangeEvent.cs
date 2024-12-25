using System;
using ActionSequencer;
using Unity.Cinemachine;
using GameFramework.CameraSystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Camera切り替え再生イベント
    /// </summary>
    public class CameraRangeEvent : RangeSequenceEvent {
        // ブレンド情報
        [Serializable]
        public struct Blend {
            [Tooltip("Blendを上書きするか")]
            public bool active;
            [Tooltip("ブレンド情報")]
            public CinemachineBlendDefinition blendDefinition;
        }

        [Tooltip("カメラグループキー")]
        public string groupKey = "";
        [Tooltip("切り替えるカメラ名")]
        public string cameraName = "";
        
        [Header("ブレンド情報")]
        [Tooltip("行き遷移時のBlend")]
        public Blend toBlend;
        [Tooltip("戻り遷移時のBlend")]
        public Blend fromBlend;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class CameraRangeEventHandler : RangeSequenceEventHandler<CameraRangeEvent> {
        private CameraManager _cameraManager;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(CameraManager manager) {
            _cameraManager = manager;
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void OnEnter(CameraRangeEvent sequenceEvent) {
            if (_cameraManager != null) {
                if (sequenceEvent.toBlend.active) {
                    _cameraManager.Activate(sequenceEvent.groupKey, sequenceEvent.cameraName, sequenceEvent.toBlend.blendDefinition);
                }
                else {
                    _cameraManager.Activate(sequenceEvent.groupKey, sequenceEvent.cameraName);
                }
            }
        }

        /// <summary>
        /// 終了時処理
        /// </summary>
        protected override void OnExit(CameraRangeEvent sequenceEvent) {
            if (_cameraManager != null) {
                if (sequenceEvent.fromBlend.active) {
                    _cameraManager.Deactivate(sequenceEvent.groupKey, sequenceEvent.cameraName, sequenceEvent.fromBlend.blendDefinition);
                }
                else {
                    _cameraManager.Deactivate(sequenceEvent.groupKey, sequenceEvent.cameraName);
                }
            }
        }

        /// <summary>
        /// キャンセル時処理
        /// </summary>
        protected override void OnCancel(CameraRangeEvent sequenceEvent) {
            OnExit(sequenceEvent);
        }
    }
}
