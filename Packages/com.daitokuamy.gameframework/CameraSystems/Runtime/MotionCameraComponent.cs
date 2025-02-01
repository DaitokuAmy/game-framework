using System;
using Unity.Cinemachine;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// シンプルなモーション制御用カメラコンポーネント
    /// [Hierarchy Image]
    /// CameraName // MotionCameraComponent
    ///   Root // RootTransform & Animator
    ///     Camera0 // Camera
    ///     Camera1 // Camera
    ///   VCam // CinemachineVirtualCamera
    /// </summary>
    public class MotionCameraComponent : MotionCameraComponentBase {
        // 初期化用コンテキスト
        [Serializable]
        public struct Context {
            public AnimationClip clip;
            public Vector3 followOffset;
        }
        
        [SerializeField, Tooltip("追従対象のカメラ")]
        private Camera[] _cameras;

        // Cinemachine用のキャッシュ
        private CinemachineTransposer _transposer;
        
        private int _cameraIndex;
        
        // フォローオフセット
        public Vector3 FollowOffset { get; set; }

        // 現在のCamera
        private Camera CurrentCamera {
            get {
                if (_cameraIndex < 0 || _cameraIndex >= _cameras.Length) {
                    return null;
                }

                return _cameras[_cameraIndex];
            }
        }

        /// <summary>
        /// パラメータのセットアップ
        /// </summary>
        /// <param name="context">再生に必要な基本情報</param>
        /// <param name="cameraIndex">使用するCameraSlot</param>
        /// <param name="time">再生時間</param>
        /// <param name="parent">追従先のTransform</param>
        /// <param name="relativePosition">相対座標</param>
        /// <param name="relativeRotation">相対向き</param>
        /// <param name="layeredTime">時間軸コントロール用</param>
        public void Setup(Context context, int cameraIndex, float time, Transform parent, Vector3 relativePosition, Quaternion relativeRotation, LayeredTime layeredTime = null) {
            if (cameraIndex < 0 || cameraIndex >= _cameras.Length) {
                Debug.LogWarning($"[{nameof(MotionCameraComponent)}]Invalid camera index. ({cameraIndex})");
                return;
            }
            
            // アニメーション部分初期化
            SetupInternal(time, context.clip, parent, relativePosition, relativeRotation, layeredTime);

            // Follow部分初期化
            _cameraIndex = cameraIndex;
            FollowOffset = context.followOffset;
            SetupVirtualCamera();
        }

        /// <summary>
        /// パラメータのセットアップ
        /// </summary>
        /// <param name="animationClip">再生対象のClip</param>
        /// <param name="cameraIndex">使用するCameraSlot</param>
        /// <param name="time">再生のオフセット時間</param>
        /// <param name="parent">追従先のTransform</param>
        /// <param name="relativePosition">相対座標</param>
        /// <param name="relativeRotation">相対向き</param>
        /// <param name="layeredTime">時間軸コントロール用</param>
        public void Setup(AnimationClip animationClip, int cameraIndex, float time, Transform parent, Vector3 relativePosition, Quaternion relativeRotation, LayeredTime layeredTime = null) {
            Setup(new Context { clip = animationClip }, cameraIndex, time, parent, relativePosition, relativeRotation, layeredTime);
        }

        /// <summary>
        /// パラメータのセットアップ
        /// </summary>
        /// <param name="animationClip">再生対象のClip</param>
        /// <param name="parent">追従先のTransform</param>
        /// <param name="relativePosition">相対座標</param>
        /// <param name="relativeRotation">相対向き</param>
        /// <param name="layeredTime">時間軸コントロール用</param>
        public void Setup(AnimationClip animationClip, Transform parent, Vector3 relativePosition, Quaternion relativeRotation, LayeredTime layeredTime = null) {
            Setup(new Context{ clip = animationClip }, 0, 0.0f, parent, relativePosition, relativeRotation, layeredTime);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();
            
            // CinemacineComponentの取得(無ければ追加)
            T AddOrGetComponent<T>()
                where T : CinemachineComponentBase {
                var comp = VirtualCamera.GetCinemachineComponent<T>();
                if (comp == null) {
                    comp = VirtualCamera.AddCinemachineComponent<T>();
                }

                return comp;
            }
            
            // 回転制御用のStageを持ったコンポーネントを削除
            var aimStageComponent = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            if (aimStageComponent != null) {
                Destroy(aimStageComponent);
            }
            
            // 操作に必要なコンポーネント追加
            _transposer = AddOrGetComponent<CinemachineTransposer>();
            _transposer.m_XDamping = 0.0f;
            _transposer.m_YDamping = 0.0f;
            _transposer.m_ZDamping = 0.0f;
            
            // カメラを全部非アクティブ化
            var cameras = GetComponentsInChildren<Camera>(true);
            foreach (var cam in cameras) {
                cam.enabled = false;
            }
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            base.UpdateInternal(deltaTime);
            
            UpdateVirtualCamera();
        }

        /// <summary>
        /// 仮想カメラの情報をセットアップ
        /// </summary>
        private void SetupVirtualCamera() {
            var currentCamera = CurrentCamera;
            if (currentCamera == null) {
                return;
            }

            VirtualCamera.Follow = currentCamera.transform;
            VirtualCamera.LookAt = null;
        }

        /// <summary>
        /// 仮想カメラの情報を更新
        /// </summary>
        private void UpdateVirtualCamera() {
            var currentCamera = CurrentCamera;
            if (currentCamera == null) {
                return;
            }
            
            VirtualCamera.m_Lens.NearClipPlane = currentCamera.nearClipPlane;
            VirtualCamera.m_Lens.FarClipPlane = currentCamera.farClipPlane;
            VirtualCamera.m_Lens.FieldOfView = currentCamera.fieldOfView;

            // 位置はTransposerで更新
            _transposer.m_FollowOffset = FollowOffset;
            
            // 向きは独自で更新
            VirtualCamera.transform.rotation = currentCamera.transform.rotation;
        }
    }
}