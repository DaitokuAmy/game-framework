using System;
using System.Collections.Generic;
using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用設定データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_config.asset", menuName = "Sample Game/Model Viewer/Config Data")]
    public class ModelViewerConfigData : ScriptableObject {
        /// <summary>
        /// マスター設定
        /// </summary>
        [Serializable]
        public class MasterInfo : IModelViewerMaster {
            [Tooltip("初期状態で読み込むアクターアセットキーのIndex")]
            public int defaultActorAssetKeyIndex = 0;
            [Tooltip("ActorDataのAssetKeyリスト")]
            public string[] actorAssetKeys = Array.Empty<string>();
            [Tooltip("初期状態で読み込む環境アセットキーのIndex")]
            public int defaultEnvironmentAssetKeyIndex = 0;
            [Tooltip("EnvironmentIDリスト")]
            public string[] environmentAssetKeys = Array.Empty<string>();

            int IModelViewerMaster.DefaultActorAssetKeyIndex => defaultActorAssetKeyIndex;
            IReadOnlyList<string> IModelViewerMaster.ActorAssetKeys => actorAssetKeys;
            int IModelViewerMaster.DefaultEnvironmentAssetKeyIndex => defaultEnvironmentAssetKeyIndex;
            IReadOnlyList<string> IModelViewerMaster.EnvironmentAssetKeys => environmentAssetKeys;
        }

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
            Vector3 IPreviewCameraMaster.StartLookAtOffset => startLookAtOffset;
            Vector3 IPreviewCameraMaster.StartAngles => startAngles;
            float IPreviewCameraMaster.StartDistance => startDistance;
        }

        [Tooltip("初期状態")]
        public MasterInfo master;
        [Tooltip("カメラ")]
        public CameraInfo camera;
    }
}