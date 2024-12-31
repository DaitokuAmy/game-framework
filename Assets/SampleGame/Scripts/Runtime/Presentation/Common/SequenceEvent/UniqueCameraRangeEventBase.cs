using System;
using ActionSequencer;
using GameFramework.CameraSystems;
using Unity.Cinemachine;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// ユニークカメラ切り替え用のイベント
    /// </summary>
    public abstract class UniqueCameraRangeEventBase : RangeSequenceEvent {
        // ブレンド情報
        [Serializable]
        public struct Blend {
            [Tooltip("カスタムブレンドを使うか")]
            public bool active;
            [Tooltip("ブレンドパラメータ")]
            public CinemachineBlendDefinition blendDefinition;
        }
        
        [Tooltip("入り用のブレンド設定")]
        public Blend toBlend;
        [Tooltip("抜け用のブレンド設定")]
        public Blend fromBlend;
    }

    /// <summary>
    /// UniqueCameraRangeEventBase用のハンドラ
    /// </summary>
    public abstract class UniqueCameraRangeEventHandlerBase<TEvent> : RangeSequenceEventHandler<TEvent>
        where TEvent : UniqueCameraRangeEventBase {
        /// <summary>カットブレンド</summary>
        private static readonly CinemachineBlendDefinition CutBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0); 
        
        private CameraManager _cameraManager;
        private string _ownerGroupKey;
        private bool _activeCamera;

        /// <summary>制御に使うCameraManager</summary>
        public CameraManager CameraManager => _cameraManager;
        /// <summary>所持者のグループキー</summary>
        public string OwnerGroupKey => _ownerGroupKey;
        /// <summary>カメラをアクティブにしているか</summary>
        public bool ActiveCamera => _activeCamera;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraManager">操作対象のCameraManager</param>
        /// <param name="cameraGroupKey">登録するカメラグループキー</param>
        protected void SetupInternal(CameraManager cameraManager, string cameraGroupKey) {
            _cameraManager = cameraManager;
            _ownerGroupKey = cameraGroupKey;
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void OnEnter(TEvent sequenceEvent) {
            if (CheckActive(sequenceEvent)) {
                ActivateCamera(sequenceEvent);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void OnUpdate(TEvent sequenceEvent, float elapsedTime) {
            if (CheckActive(sequenceEvent)) {
                ActivateCamera(sequenceEvent, true);
            }
            else {
                DeactivateCamera(sequenceEvent, true);
            }
        }

        /// <summary>
        /// 終了時処理
        /// </summary>
        protected override void OnExit(TEvent sequenceEvent) {
            DeactivateCamera(sequenceEvent);
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void OnCancel(TEvent sequenceEvent) => OnExit(sequenceEvent);

        /// <summary>
        /// カメラ名の取得
        /// </summary>
        protected abstract void GetCameraName(TEvent sequenceEvent, out string groupKey, out string cameraName);

        /// <summary>
        /// アクティブ状態かチェック
        /// </summary>
        protected abstract bool CheckActive(TEvent sequenceEvent);

        /// <summary>
        /// アクティブ化された時の処理
        /// </summary>
        protected virtual void OnActivated(TEvent sequenceEvent) {}
        
        /// <summary>
        /// 非アクティブ化された時の処理
        /// </summary>
        protected virtual void OnDeactivated(TEvent sequenceEvent) {}

        /// <summary>
        /// カメラのアクティブ化
        /// </summary>
        private void ActivateCamera(TEvent sequenceEvent, bool forceCut = false) {
            if (_activeCamera) {
                return;
            }

            GetCameraName(sequenceEvent, out var groupKey, out var cameraName);
            if (sequenceEvent.toBlend.active || forceCut) {
                _cameraManager.Activate(groupKey, cameraName, forceCut ? CutBlend : sequenceEvent.toBlend.blendDefinition);
            }
            else {
                _cameraManager.Activate(groupKey, cameraName); 
            }

            _activeCamera = true;

            OnActivated(sequenceEvent);
        }

        /// <summary>
        /// カメラの非アクティブ化
        /// </summary>
        private void DeactivateCamera(TEvent sequenceEvent, bool forceCut = false) {
            if (!_activeCamera) {
                return;
            }

            GetCameraName(sequenceEvent, out var groupKey, out var cameraName);
            if (sequenceEvent.fromBlend.active || forceCut) {
                _cameraManager.Deactivate(groupKey, cameraName, forceCut ? CutBlend : sequenceEvent.fromBlend.blendDefinition);
            }
            else {
                _cameraManager.Deactivate(groupKey, cameraName);
            }

            _activeCamera = false;
            
            OnDeactivated(sequenceEvent);
        }
    }
}