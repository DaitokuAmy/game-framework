using Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// カメラターゲットの文字列指定
    /// </summary>
    public class CameraTarget : MonoBehaviour {
        [SerializeField, Tooltip("Followに指定するTarget名")]
        private string _followTargetName = "";
        [SerializeField, Tooltip("LookAtに指定するTarget名")]
        private string _lookAtTargetName = "";

        /// <summary>Followに指定するTargetPoint名</summary>
        public string FollowTargetName => _followTargetName;
        /// <summary>LookAtに指定するTargetPoint名</summary>
        public string LookAtTargetName => _lookAtTargetName;

        /// <summary>
        /// カメラターゲットのセットアップ
        /// </summary>
        /// <param name="cameraManager">TargetPointを管理しているCameraManager</param>
        /// <param name="virtualCamera">依存を入れるためのVirtualCamera</param>
        public void SetupCameraTarget(CameraManager cameraManager, CinemachineVirtualCameraBase virtualCamera) {
            if (cameraManager == null || virtualCamera == null) {
                return;
            }

            if (string.IsNullOrEmpty(FollowTargetName)) {
                virtualCamera.Follow = null;
            }
            else {
                virtualCamera.Follow = cameraManager.GetTargetPoint(FollowTargetName);
            }

            if (string.IsNullOrEmpty(LookAtTargetName)) {
                virtualCamera.LookAt = null;
            }
            else {
                virtualCamera.LookAt = cameraManager.GetTargetPoint(LookAtTargetName);
            }
        }
    }
}