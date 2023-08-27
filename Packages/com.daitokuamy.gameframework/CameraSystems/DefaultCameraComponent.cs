using System;
using Cinemachine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// 標準的なカメラコンポーネント
    /// </summary>
    public class DefaultCameraComponent : ICameraComponent {
        // アクティブ状態
        bool ICameraComponent.IsActive => VirtualCamera != null && VirtualCamera.gameObject.activeSelf;

        // 基本カメラ
        ICinemachineCamera ICameraComponent.BaseCamera => VirtualCamera;

        // 制御対象の仮想カメラ
        public CinemachineVirtualCameraBase VirtualCamera { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="virtualCamera">制御対象の仮想カメラ</param>
        public DefaultCameraComponent(CinemachineVirtualCameraBase virtualCamera) {
            VirtualCamera = virtualCamera;
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICameraComponent.Initialize(CameraManager cameraManager) {
            // Target指定が文字列で行われていたら取り直す
            var cameraTarget = VirtualCamera.GetComponent<CameraTarget>();
            if (cameraTarget != null) {
                cameraTarget.SetupCameraTarget(cameraManager, VirtualCamera);
            }
        }
        
        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        void ICameraComponent.Activate() {
            if (VirtualCamera == null) {
                return;
            }

            if (((ICameraComponent)this).IsActive) {
                return;
            }

            VirtualCamera.gameObject.SetActive(true);
        }

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void ICameraComponent.Deactivate() {
            if (VirtualCamera == null) {
                return;
            }

            if (!((ICameraComponent)this).IsActive) {
                return;
            }

            VirtualCamera.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新
        /// </summary>
        void ICameraComponent.Update(float deltaTime) {
        }
    }
}