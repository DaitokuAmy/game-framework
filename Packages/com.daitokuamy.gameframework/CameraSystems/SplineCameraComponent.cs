using System;
using Cinemachine;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Splines;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// スプライン制御用カメラコンポーネントの規定
    /// </summary>
    public class SplineCameraComponent : SerializedCameraComponent<CinemachineVirtualCamera> {
        // 初期化用コンテキスト
        [Serializable]
        public struct Context {
            [Tooltip("再生するスプラインのPrefab")]
            public SplineContainer splinePrefab;
            [Tooltip("再生時間")]
            public float duration;
            [Tooltip("時間進捗カーブ")]
            public AnimationCurve timeCurve;
        }

        private SplineContainer _splineContainer;
        private float _duration;
        private AnimationCurve _timeCurve;
        
        private float _currentTime;
        private LayeredTime _layeredTime;

        private Transform _parentTransform;
        private Vector3 _relativePosition;
        private Quaternion _relativeRotation;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="context">再生に必要な情報</param>
        /// <param name="time">再生開始時間</param>
        /// <param name="parent">追従対象のTransform</param>
        /// <param name="relativePosition">相対座標</param>
        /// <param name="relativeRotation">相対向き</param>
        /// <param name="layeredTime">時間コントロール</param>
        public void Setup(Context context, float time, Transform parent, Vector3 relativePosition, Quaternion relativeRotation, LayeredTime layeredTime = null) {
            _splineContainer = context.splinePrefab;
            _layeredTime = layeredTime;
            _currentTime = time;
            _duration = context.duration;
            _timeCurve = context.timeCurve;
            _parentTransform = parent;
            _relativePosition = relativePosition;
            _relativeRotation = relativeRotation;
            
            ApplyCameraTransform();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // Body制御用のStageを持ったコンポーネントを削除
            var bodyStageComponent = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (bodyStageComponent != null) {
                Destroy(bodyStageComponent);
            }
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            var dt = _layeredTime != null ? _layeredTime.DeltaTime : deltaTime;
            _currentTime += deltaTime;
            ApplyCameraTransform();
        }

        /// <summary>
        /// VirtualCameraのTransformの更新
        /// </summary>
        private void ApplyCameraTransform() {
            if (_splineContainer == null || _splineContainer.Spline == null || _splineContainer.Spline.Count <= 0) {
                return;
            }

            var rate = _duration > 0.001f ? Mathf.Min(1.0f, _currentTime / _duration) : 1.0f;
            rate = _timeCurve != null && _timeCurve.keys.Length > 1 ? _timeCurve.Evaluate(rate) : rate;

            _splineContainer.Spline.Evaluate(rate, out var splinePos, out _, out _);

            var pos = _relativePosition + _relativeRotation * splinePos;
            if (_parentTransform != null) {
                pos = _parentTransform.TransformPoint(pos);
            }
            
            VirtualCamera.transform.position = pos;
        }
    }
}