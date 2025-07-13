using Unity.Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// ビューワー用のカメラコンポーネント
    /// </summary>
    public class PreviewCameraComponent : SerializedCameraComponent<CinemachineCamera> {
        [SerializeField, Tooltip("距離")]
        private float _distance = 10.0f;
        [SerializeField, Tooltip("注視点オフセット")]
        private Vector3 _lookAtOffset;
        [SerializeField, Tooltip("X軸回転"), Range(-89.9f, 89.9f)]
        private float _angleX;
        [SerializeField, Tooltip("Y軸回転"), Range(0.0f, 360.0f)]
        private float _angleY;

        // FOV
        public float Fov {
            get => VirtualCamera.Lens.FieldOfView;
            set => VirtualCamera.Lens.FieldOfView = value;
        }
        // 距離
        public float Distance {
            get => _distance;
            set => _distance = Mathf.Max(0.01f, value);
        }
        // X角度
        public float AngleX {
            get => _angleX;
            set => _angleX = Mathf.Clamp(value, -89.9f, 89.9f);
        }
        // Y角度
        public float AngleY {
            get => _angleY;
            set => _angleY = Mathf.Repeat(value, 360.0f);
        }
        // 注視対象
        public Transform LookAt {
            get => VirtualCamera.LookAt;
            set => VirtualCamera.LookAt = value;
        }
        // 注視点オフセット
        public Vector3 LookAtOffset {
            get => _lookAtOffset;
            set => _lookAtOffset = value;
        }
        // ニアクリップ
        public float NearClip {
            get => VirtualCamera.Lens.NearClipPlane;
            set => VirtualCamera.Lens.NearClipPlane = Mathf.Clamp(value, 0.01f, FarClip);
        }
        // ファークリップ
        public float FarClip {
            get => VirtualCamera.Lens.FarClipPlane;
            set => VirtualCamera.Lens.FarClipPlane = Mathf.Clamp(value, NearClip, float.MaxValue);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // 基本的なコンポーネントは削除する
            var bodyComponent = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (bodyComponent != null) {
                Destroy(bodyComponent);
            }

            var aimComponent = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            if (aimComponent != null) {
                Destroy(aimComponent);
            }
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            // 相対位置計算
            var relativePosition = CalcRelativePosition();

            // 注視位置計算
            var lookAtPosition = CalcLookAtPosition();

            // 姿勢に反映
            var trans = VirtualCamera.transform;
            trans.position = lookAtPosition + relativePosition;
            trans.LookAt(lookAtPosition);
        }

        /// <summary>
        /// 注視座標からの相対位置を計算
        /// </summary>
        public Vector3 CalcRelativePosition() {
            return Quaternion.Euler(_angleX, _angleY, 0.0f) * Vector3.back * _distance;
        }

        /// <summary>
        /// 注視座標の計算
        /// </summary>
        public Vector3 CalcLookAtPosition() {
            var lookAt = VirtualCamera.LookAt;
            var basePosition = lookAt != null ? lookAt.position : Vector3.zero;
            return basePosition + Quaternion.Euler(0.0f, _angleY, 0.0f) * _lookAtOffset;
        }
    }
}