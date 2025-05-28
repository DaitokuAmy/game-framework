using Unity.Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// Cameraのパラメータを仮想カメラに転送するコンポーネント
    /// </summary>
    [ExecuteAlways]
    public class VirtualCameraTransporter : MonoBehaviour {
        [SerializeField, Tooltip("制御元になっているカメラ")]
        private Camera _source;
        [SerializeField, Tooltip("転送先の仮想カメラ")]
        private CinemachineCamera _destination;

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
            if (_source == null || _destination == null) {
                return;
            }

            _destination.Lens.NearClipPlane = _source.nearClipPlane;
            _destination.Lens.FarClipPlane = _source.farClipPlane;

            if (_source.orthographic) {
                _destination.Lens.OrthographicSize = _source.orthographicSize;
                _destination.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            }
            else if (_source.usePhysicalProperties) {
                _destination.Lens.FieldOfView = _source.fieldOfView;
                _destination.Lens.PhysicalProperties.FocusDistance = _source.focusDistance;
                _destination.Lens.PhysicalProperties.SensorSize = _source.sensorSize;
                _destination.Lens.PhysicalProperties.LensShift = _source.lensShift;
                _destination.Lens.PhysicalProperties.BarrelClipping = _source.barrelClipping;
                _destination.Lens.PhysicalProperties.Curvature = _source.curvature;
                _destination.Lens.PhysicalProperties.Anamorphism = _source.anamorphism;
                _destination.Lens.PhysicalProperties.Aperture = _source.aperture;
                _destination.Lens.PhysicalProperties.ShutterSpeed = _source.shutterSpeed;
                _destination.Lens.PhysicalProperties.Iso = _source.iso;
                _destination.Lens.PhysicalProperties.BladeCount = _source.bladeCount;
                _destination.Lens.PhysicalProperties.GateFit = _source.gateFit;
                _destination.Lens.ModeOverride = LensSettings.OverrideModes.Physical;
            }
            else {
                _destination.Lens.FieldOfView = _source.fieldOfView;
                _destination.Lens.ModeOverride = LensSettings.OverrideModes.None;
            }

            var cameraTrans = _source.transform;
            var targetTrans = _destination.transform;
            targetTrans.position = cameraTrans.position;
            targetTrans.rotation = cameraTrans.rotation;
        }
    }
}