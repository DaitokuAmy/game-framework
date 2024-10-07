using System;
using Cinemachine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// 標準的なカメラコンポーネント
    /// </summary>
    public class DefaultCameraComponent : ICameraComponent {
        private int _defaultPriority;
        
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
            
            // 初期化時点でのPriorityをキャッシュ
            if (VirtualCamera != null) {
                _defaultPriority = VirtualCamera.Priority;
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

        /// <summary>
        /// プライオリティのセット(上書き用)
        /// </summary>
        void ICameraComponent.SetPriority(int priority) {
            if (VirtualCamera == null) {
                return;
            }

            VirtualCamera.Priority = priority;
        }

        /// <summary>
        /// プライオリティのリセット
        /// </summary>
        void ICameraComponent.ResetPriority() {
            if (VirtualCamera == null) {
                return;
            }

            VirtualCamera.Priority = _defaultPriority;
        }
    }
}