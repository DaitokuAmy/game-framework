using Unity.Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// Cameraのパラメータを仮想カメラに転送するコンポーネント
    /// </summary>
    [ExecuteAlways]
    public class VirtualCameraTransporter : MonoBehaviour {
        [SerializeField, Tooltip("制御元になっているカメラ")]
        private Camera _camera;
        [SerializeField, Tooltip("転送先の仮想カメラ")]
        private CinemachineVirtualCamera _target;

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            ApplyCamera();
        }

        /// <summary>
        /// カメラの情報を反映
        /// </summary>
        private void ApplyCamera() {
            if (_camera == null || _target == null) {
                return;
            }

            _camera.enabled = false;
            _target.m_Lens.NearClipPlane = _camera.nearClipPlane;
            _target.m_Lens.FarClipPlane = _camera.farClipPlane;
            _target.m_Lens.GateFit = _camera.gateFit;

            if (_camera.orthographic) {
                _target.m_Lens.OrthographicSize = _camera.orthographicSize;
                _target.m_Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            }
            else if (_camera.usePhysicalProperties) {
                _target.m_Lens.FieldOfView = _camera.fieldOfView;
                _target.m_Lens.FocusDistance = _camera.focusDistance;
                _target.m_Lens.m_SensorSize = _camera.sensorSize;
                _target.m_Lens.LensShift = _camera.lensShift;
                _target.m_Lens.ModeOverride = LensSettings.OverrideModes.Physical;
            }
            else {
                _target.m_Lens.FieldOfView = _camera.fieldOfView;
                _target.m_Lens.ModeOverride = LensSettings.OverrideModes.None;
            }

            var cameraTrans = _camera.transform;
            var targetTrans = _target.transform;
            targetTrans.position = cameraTrans.position;
            targetTrans.rotation = cameraTrans.rotation;
        }
    }
}