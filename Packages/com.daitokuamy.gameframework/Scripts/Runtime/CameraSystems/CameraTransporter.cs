using UnityEngine;

namespace SpearEngine {
    /// <summary>
    /// Cameraの基本情報を転送するクラス
    /// </summary>
    [ExecuteAlways]
    public class CameraTransporter : MonoBehaviour {
        [SerializeField, Tooltip("転送元のカメラ")]
        private Camera _source;
        [SerializeField, Tooltip("転送先のカメラ")]
        private Camera _destination;

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
            
            _destination.nearClipPlane = _source.nearClipPlane;
            _destination.farClipPlane = _source.farClipPlane;

            _destination.orthographicSize = _source.orthographicSize;
            _destination.orthographic = _source.orthographic;
            
            _destination.usePhysicalProperties = _source.usePhysicalProperties;
            _destination.fieldOfView = _source.fieldOfView;
            _destination.focusDistance = _source.focusDistance;
            _destination.sensorSize = _source.sensorSize;
            _destination.lensShift = _source.lensShift;
            _destination.barrelClipping = _source.barrelClipping;
            _destination.curvature = _source.curvature;
            _destination.anamorphism = _source.anamorphism;
            _destination.aperture = _source.aperture;
            _destination.shutterSpeed = _source.shutterSpeed;
            _destination.iso = _source.iso;
            _destination.bladeCount = _source.bladeCount;
            _destination.gateFit = _source.gateFit;
            _destination.fieldOfView = _source.fieldOfView;

            var cameraTrans = _source.transform;
            var targetTrans = _destination.transform;
            targetTrans.position = cameraTrans.position;
            targetTrans.rotation = cameraTrans.rotation;
        }
    }
}