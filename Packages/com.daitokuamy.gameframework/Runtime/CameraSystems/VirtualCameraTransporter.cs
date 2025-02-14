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
        private CinemachineCamera _target;

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
            _target.Lens.NearClipPlane = _camera.nearClipPlane;
            _target.Lens.FarClipPlane = _camera.farClipPlane;

            if (_camera.orthographic) {
                _target.Lens.OrthographicSize = _camera.orthographicSize;
                _target.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            }
            else if (_camera.usePhysicalProperties) {
                _target.Lens.FieldOfView = _camera.fieldOfView;
                _target.Lens.PhysicalProperties.FocusDistance = _camera.focusDistance;
                _target.Lens.PhysicalProperties.SensorSize = _camera.sensorSize;
                _target.Lens.PhysicalProperties.LensShift = _camera.lensShift;
                _target.Lens.PhysicalProperties.BarrelClipping = _camera.barrelClipping;
                _target.Lens.PhysicalProperties.Curvature = _camera.curvature;
                _target.Lens.PhysicalProperties.Anamorphism = _camera.anamorphism;
                _target.Lens.PhysicalProperties.Aperture = _camera.aperture;
                _target.Lens.PhysicalProperties.ShutterSpeed = _camera.shutterSpeed;
                _target.Lens.PhysicalProperties.Iso = _camera.iso;
                _target.Lens.PhysicalProperties.BladeCount = _camera.bladeCount;
                _target.Lens.PhysicalProperties.GateFit = _camera.gateFit;
                _target.Lens.ModeOverride = LensSettings.OverrideModes.Physical;
            }
            else {
                _target.Lens.FieldOfView = _camera.fieldOfView;
                _target.Lens.ModeOverride = LensSettings.OverrideModes.None;
            }

            var cameraTrans = _camera.transform;
            var targetTrans = _target.transform;
            targetTrans.position = cameraTrans.position;
            targetTrans.rotation = cameraTrans.rotation;
        }
    }
}