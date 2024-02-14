#if USE_SPLINES

using Cinemachine;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Splines;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// スプライン制御用カメラコンポーネントの規定
    /// </summary>
    public class SplineCameraComponentBase : SerializedCameraComponent<CinemachineVirtualCamera> {
        [SerializeField, Tooltip("再生するSpline")]
        private SplineContainer _splineContainer;
        [SerializeField, Tooltip("再生時間")]
        private float _duration = 1.0f;

        private float _currentTime;
        private LayeredTime _layeredTime;

        private Transform _parentTransform;
        private Vector3 _relativePosition;
        private Quaternion _relativeRotation;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // 移動制御用のStageを持ったコンポーネントを削除
            var bodyComponent = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (bodyComponent != null) {
                Destroy(bodyComponent);
            }
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal() {
            _currentTime = 0.0f;
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            if (_splineContainer == null || _duration <= float.Epsilon) {
                return;
            }

            if (_currentTime >= _duration) {
                return;
            }

            _currentTime += _layeredTime?.DeltaTime ?? deltaTime;
            ApplyTransform();
        }

        /// <summary>
        /// パラメータのセットアップ
        /// </summary>
        /// <param name="parent">追従先のTransform</param>
        /// <param name="relativePosition">相対座標</param>
        /// <param name="relativeRotation">相対向き</param>
        /// <param name="layeredTime">時間軸コントロール用</param>
        public void Setup(Transform parent, Vector3 relativePosition, Quaternion relativeRotation, LayeredTime layeredTime = null) {
            _layeredTime = layeredTime;
            _currentTime = 0.0f;
            
            _parentTransform = parent;
            _relativePosition = relativePosition;
            _relativeRotation = relativeRotation;

            ApplyTransform();
        }

        /// <summary>
        /// RootTransformの更新
        /// </summary>
        private void ApplyTransform() {
            var rate = Mathf.Min(1.0f, _currentTime / _duration);
            _splineContainer.Spline.Evaluate(rate, out var splinePos, out var splineTangent, out var splineUpVec);
            var splineRot = Quaternion.LookRotation(splineTangent, splineUpVec);
            
            var pos = _relativePosition + _relativeRotation * splinePos;
            var rot = _relativeRotation * splineRot;
            if (_parentTransform != null) {
                pos = _parentTransform.TransformPoint(pos);
                rot = _parentTransform.rotation * _relativeRotation;
            }
            
            VirtualCamera.transform.SetPositionAndRotation(pos, rot);
        }
        
        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            if (Application.isEditor) {
                _currentTime = 0.0f;
            }
        }
    }
}

#endif