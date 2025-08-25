using System;
using ActionSequencer;
using Unity.Cinemachine;
using GameFramework.CameraSystems;
using GameFramework.Core;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// SplineCamera切り替え再生イベント
    /// </summary>
    public class SplineCameraRangeEvent : RangeSequenceEvent {
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

        [Header("スプライン情報")]
        [Tooltip("スプライン再生情報")]
        public SplineCameraComponent.Context context;
        [Tooltip("アニメーションの再生開始時間")]
        public float startTime;
        [Tooltip("Rootを追従させるか")]
        public bool constraintRoot;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class SplineCameraRangeEventHandler : RangeSequenceEventHandler<SplineCameraRangeEvent> {
        private CameraManager _cameraManager;
        private Transform _root;
        private LayeredTime _layeredTime;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(CameraManager manager, Transform root, LayeredTime layeredTime = null) {
            _cameraManager = manager;
            _root = root;
            _layeredTime = layeredTime;
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void OnEnter(SplineCameraRangeEvent sequenceEvent) {
            if (_cameraManager != null) {
                var component = _cameraManager.GetCameraComponent<SplineCameraComponent>(sequenceEvent.cameraName);
                if (component != null) {
                    var parent = _root;
                    var relativePos = Vector3.zero;
                    var relativeRot = Quaternion.identity;
                    if (!sequenceEvent.constraintRoot) {
                        parent = null;
                        if (_root != null) {
                            relativePos = _root.position;
                            relativeRot = _root.rotation;
                        }
                    }

                    component.Setup(sequenceEvent.context, sequenceEvent.startTime, parent, relativePos, relativeRot, _layeredTime);
                }

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
        protected override void OnExit(SplineCameraRangeEvent sequenceEvent) {
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
        protected override void OnCancel(SplineCameraRangeEvent sequenceEvent) {
            OnExit(sequenceEvent);
        }
    }
}