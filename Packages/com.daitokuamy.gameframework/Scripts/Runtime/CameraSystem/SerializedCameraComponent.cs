using Unity.Cinemachine;
using UnityEngine;
using System;

namespace GameFramework.CameraSystem {
    /// <summary>
    /// シリアライズ想定のカメラコンポーネント
    /// </summary>
    public abstract class SerializedCameraComponent<TCamera> : MonoBehaviour, ICameraComponent
        where TCamera : CinemachineVirtualCameraBase {
        [SerializeField, Tooltip("制御対象の仮想カメラ")]
        private TCamera _virtualCamera;

        private int _defaultPriority;

        // アクティブ状態
        bool ICameraComponent.IsActive => gameObject.activeSelf;

        // 基本カメラ
        ICinemachineCamera ICameraComponent.BaseCamera => _virtualCamera;
        /// <summary>仮想カメラ</summary>
        protected TCamera VirtualCamera => _virtualCamera;
        
        /// <inheritdoc/>
        void ICameraComponent.Initialize(CameraManager cameraManager) {
            // Target指定が文字列で行われていたら取り直す
            var cameraTarget = VirtualCamera.GetComponent<CameraTarget>();
            if (cameraTarget != null) {
                cameraTarget.SetupCameraTarget(cameraManager, VirtualCamera);
            }
            
            // 初期化時点でのPriorityをキャッシュ
            if (_virtualCamera != null) {
                _defaultPriority = _virtualCamera.Priority;
            }
            
            InitializeInternal();
        }

        /// <inheritdoc/>
        void IDisposable.Dispose() {
            DisposeInternal();
        }
        
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        void ICameraComponent.Update(float deltaTime) {
            if (_virtualCamera == null) {
                return;
            }
            
            UpdateInternal(deltaTime);
        }

        /// <inheritdoc/>
        void ICameraComponent.SetPriority(int priority) {
            if (_virtualCamera == null) {
                return;
            }

            _virtualCamera.Priority = priority;
        }

        /// <inheritdoc/>
        void ICameraComponent.ResetPriority() {
            if (_virtualCamera == null) {
                return;
            }

            _virtualCamera.Priority = _defaultPriority;
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
