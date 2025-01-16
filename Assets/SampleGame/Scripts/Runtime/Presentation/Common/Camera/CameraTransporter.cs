using UnityEngine;

namespace SampleGame.Presentation {
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

            _destination.fieldOfView = _source.fieldOfView;
            _destination.nearClipPlane = _source.nearClipPlane;
            _destination.farClipPlane = _source.farClipPlane;
            _destination.transform.position = _source.transform.position;
            _destination.transform.rotation = _source.transform.rotation;

            if (_destination.enabled != _source.enabled) {
                _destination.enabled = _source.enabled;
            }
        }
    }
}