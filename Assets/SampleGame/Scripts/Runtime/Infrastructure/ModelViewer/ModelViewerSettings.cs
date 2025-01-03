using System;
using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用の設定
    /// </summary>
    public class ModelViewerSettings : MonoBehaviour {
        /// <summary>
        /// カメラ設定
        /// </summary>
        [Serializable]
        public class CameraInfo : IPreviewCameraControllerContext {
            [SerializeField, Tooltip("左ドラック速度")]
            private float _mouseLeftDeltaSpeed = 1.0f;
            [SerializeField, Tooltip("中ドラック速度")]
            private float _mouseMiddleDeltaSpeed = 0.01f;
            [SerializeField, Tooltip("右ドラック速度")]
            private float _mouseRightDeltaSpeed = 0.01f;
            [SerializeField, Tooltip("スクロールドラック速度")]
            private float _mouseScrollSpeed = 0.01f;

            public float MouseLeftDeltaSpeed => _mouseLeftDeltaSpeed;
            public float MouseMiddleDeltaSpeed => _mouseMiddleDeltaSpeed;
            public float MouseRightDeltaSpeed => _mouseRightDeltaSpeed;
            public float MouseScrollSpeed => _mouseScrollSpeed;
        }
        
        [SerializeField, Tooltip("カメラ情報")]
        private CameraInfo _camera;
        
        /// <summary>カメラ情報</summary>
        public CameraInfo Camera => _camera;
    }
}
