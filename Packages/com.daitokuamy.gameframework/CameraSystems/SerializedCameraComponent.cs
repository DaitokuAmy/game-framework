using Cinemachine;
using UnityEngine;
using System;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// シリアライズ想定のカメラコンポーネント
    /// </summary>
    public abstract class SerializedCameraComponent<TCamera> : MonoBehaviour, ICameraComponent
        where TCamera : CinemachineVirtualCameraBase {
        [SerializeField, Tooltip("制御対象の仮想カメラ")]
        private TCamera _virtualCamera;

        // アクティブ状態
        bool ICameraComponent.IsActive => gameObject.activeSelf;

        // 基本カメラ
        ICinemachineCamera ICameraComponent.BaseCamera => _virtualCamera;
        // 仮想カメラ
        protected TCamera VirtualCamera => _virtualCamera;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICameraComponent.Initialize(CameraManager cameraManager) {
            // Target指定が文字列で行われていたら取り直す
            var cameraTarget = VirtualCamera.GetComponent<CameraTarget>();
            if (cameraTarget != null) {
                cameraTarget.SetupCameraTarget(cameraManager, VirtualCamera);
            }
            
            InitializeInternal();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            DisposeInternal();
        }
        
        /// <summary>
        /// カメラアクティブ時処理
        /// カメラアクティブ時処理
        /// </summary>
        void ICameraComponent.Activate() {
            if (_virtualCamera == null) {
                return;
            }

            if (((ICameraComponent)this).IsActive) {
                return;
            }

            gameObject.SetActive(true);
            ActivateInternal();
        }

        /// <summary>
        /// カメラ非アクティブ時処理
        /// </summary>
        void ICameraComponent.Deactivate() {
            if (_virtualCamera == null) {
                return;
            }

            if (!((ICameraComponent)this).IsActive) {
                return;
            }
            
            DeactivateInternal();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        void ICameraComponent.Update(float deltaTime) {
            if (_virtualCamera == null) {
                return;
            }
            
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeInternal() {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected virtual void ActivateInternal() {
        }
        
        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected virtual void DeactivateInternal() {
        }
        
        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void UpdateInternal(float deltaTime) {
        }
    }
}