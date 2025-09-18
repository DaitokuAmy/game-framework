using System;
using SampleGame.Domain.ModelViewer;
using Unity.Mathematics;
using UnityEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用設定データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_config.asset", menuName = "Sample Game/Model Viewer/Config Data")]
    public class ModelViewerConfigData : ScriptableObject {
        /// <summary>
        /// カメラ設定
        /// </summary>
        [Serializable]
        public class CameraInfo : IPreviewCameraMaster {
            [Tooltip("角度制御速度")]
            public float angleControlSpeed = 1.0f;
            [Tooltip("注視点オフセット制御速度")]
            public float lookAtOffsetControlSpeed = 0.01f;
            [Tooltip("移動制御速度")]
            public float distanceControlSpeed = 0.01f;
            [Tooltip("スクロールによる移動速度制御")]
            public float scrollDistanceControlSpeed = 0.01f;
            [Tooltip("初期注視オフセット")]
            public Vector3 startLookAtOffset = Vector3.zero;
            [Tooltip("初期角度")]
            public Vector3 startAngles = Vector3.zero;
            [Tooltip("初期距離")]
            public float startDistance = 10.0f;

            float IPreviewCameraMaster.AngleControlSpeed => angleControlSpeed;
            float IPreviewCameraMaster.LookAtOffsetControlSpeed => lookAtOffsetControlSpeed;
            float IPreviewCameraMaster.DistanceControlSpeed => distanceControlSpeed;
            float IPreviewCameraMaster.ScrollDistanceControlDistanceControlSpeed => scrollDistanceControlSpeed;
            float3 IPreviewCameraMaster.StartLookAtOffset => startLookAtOffset;
            float3 IPreviewCameraMaster.StartAngles => startAngles;
            float IPreviewCameraMaster.StartDistance => startDistance;
        }
        
        [Tooltip("カメラ")]
        public CameraInfo camera;
    }
}