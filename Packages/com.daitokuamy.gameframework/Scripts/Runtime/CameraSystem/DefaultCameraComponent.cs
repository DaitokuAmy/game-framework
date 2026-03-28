using System;
using Unity.Cinemachine;

namespace GameFramework.CameraSystem {
    /// <summary>
    /// 標準的なカメラコンポーネント
    /// </summary>
    public class DefaultCameraComponent : ICameraComponent {
        private int _defaultPriority;
        
        // アクティブ状態
        bool ICameraComponent.IsActive => VirtualCamera != null && VirtualCamera.gameObject.activeSelf;

        // 基本カメラ
        ICinemachineCamera ICameraComponent.BaseCamera => VirtualCamera;

        /// <summary>制御対象の仮想カメラ</summary>
        public CinemachineVirtualCameraBase VirtualCamera { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="virtualCamera">制御対象の仮想カメラ</param>
        public DefaultCameraComponent(CinemachineVirtualCameraBase virtualCamera) {
            VirtualCamera = virtualCamera;
        }
        
        /// <inheritdoc/>
        void ICameraComponent.Initialize(CameraManager cameraManager) {
            // Target指定が文字列で行われていたら取り直す
            var cameraTarget = VirtualCamera.GetComponent<CameraTarget>();
            if (cameraTarget != null) {
                cameraTarget.SetupCameraTarget(cameraManager, VirtualCamera);
            }
            
            // 初期化時点でのPriorityをキャッシュ
            if (VirtualCamera != null) {
                _defaultPriority = VirtualCamera.Priority;
            }
        }
        
        /// <inheritdoc/>
        void IDisposable.Dispose() {
        }

        /// <inheritdoc/>
        void ICameraComponent.Activate() {
            if (VirtualCamera == null) {
                return;
            }

            if (((ICameraComponent)this).IsActive) {
                return;
            }

            VirtualCamera.gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        void ICameraComponent.Deactivate() {
            if (VirtualCamera == null) {
                return;
            }

            if (!((ICameraComponent)this).IsActive) {
                return;
            }

            VirtualCamera.gameObject.SetActive(false);
        }

        /// <inheritdoc/>
        void ICameraComponent.Update(float deltaTime) {
        }

        /// <inheritdoc/>
        void ICameraComponent.SetPriority(int priority) {
            if (VirtualCamera == null) {
                return;
            }

            VirtualCamera.Priority = priority;
        }

        /// <inheritdoc/>
        void ICameraComponent.ResetPriority() {
            if (VirtualCamera == null) {
                return;
            }

            VirtualCamera.Priority = _defaultPriority;
        }
    }
}
