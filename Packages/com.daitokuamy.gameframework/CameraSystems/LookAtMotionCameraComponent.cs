using System;
using Unity.Cinemachine;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// LookAtベースのモーション制御用カメラコンポーネント
    /// [Hierarchy Image]
    /// CameraName // LookAtMotionCameraComponent
    ///   Root // RootTransform & Animator
    ///     Slot0
    ///       Camera0 // Camera & Follow
    ///       LookAt0 // LookAt
    ///     Slot1
    ///       Camera1 // Camera & Follow
    ///       LookAt1 // LookAt
    ///   VCam // CinemachineVirtualCamera
    /// </summary>
    public class LookAtMotionCameraComponent : MotionCameraComponentBase {
        // 初期化用コンテキスト
        [Serializable]
        public struct Context {
            public AnimationClip clip;
            public Vector2 screenOffset;
            public Vector3 followOffset;
            public Vector3 lookAtOffset;
        }
        
        /// <summary>
        /// カメラ追従用のスロット
        /// </summary>
        [Serializable]
        private class Slot {
            [Tooltip("FOVの情報を抜き出すためのカメラ")]
            public Camera camera;
            [Tooltip("カメラ位置を決めるためのTransform")]
            public Transform follow;
            [Tooltip("カメラ注視点を決めるためのTransform")]
            public Transform lookAt;
        }
        
        [SerializeField, Tooltip("カメラ追従対象の設定")]
        private Slot[] _slots;

        // Cinemachine用のキャッシュ
        private CinemachineTransposer _transposer;
        private CinemachineComposer _composer;

        private int _slotIndex;

        // 注視点オフセット
        public Vector3 LookAtOffset { get; set; }
        // フォローオフセット
        public Vector3 FollowOffset { get; set; }
        // スクリーンオフセット(中央:0,0)
        public Vector2 ScreenOffset { get; set; }

        // 現在のSlot
        private Slot CurrentSlot {
            get {
                if (_slotIndex < 0 || _slotIndex >= _slots.Length) {
                    return null;
                }

                return _slots[_slotIndex];
            }
        }

        /// <summary>
        /// パラメータのセットアップ
        /// </summary>
        /// <param name="context">再生に必要な基本情報</param>
        /// <param name="slotIndex">使用するCameraSlot</param>
        /// <param name="time">再生時間</param>
        /// <param name="parent">追従先のTransform</param>
        /// <param name="relativePosition">相対座標</param>
        /// <param name="relativeRotation">相対向き</param>
        /// <param name="layeredTime">時間軸コントロール用</param>
        public void Setup(Context context, int slotIndex, float time, Transform parent, Vector3 relativePosition, Quaternion relativeRotation, LayeredTime layeredTime = null) {
            if (slotIndex < 0 || slotIndex >= _slots.Length) {
                Debug.LogWarning($"[{nameof(LookAtMotionCameraComponent)}]Invalid slot index. ({slotIndex})");
                return;
            }
            
            // アニメーション部分初期化
            SetupInternal(time, context.clip, parent, relativePosition, relativeRotation, layeredTime);

            // Follow/LookAt部分初期化
            _slotIndex = slotIndex;
            ScreenOffset = context.screenOffset;
            FollowOffset = context.followOffset;
            LookAtOffset = context.lookAtOffset;
            SetupVirtualCamera();
        }

        /// <summary>
        /// パラメータのセットアップ
        /// </summary>
        /// <param name="animationClip">再生対象のClip</param>
        /// <param name="slotIndex">使用するCameraSlot</param>
        /// <param name="time">再生時間</param>
        /// <param name="parent">追従先のTransform</param>
        /// <param name="relativePosition">相対座標</param>
        /// <param name="relativeRotation">相対向き</param>
        /// <param name="layeredTime">時間軸コントロール用</param>
        public void Setup(AnimationClip animationClip, int slotIndex, float time, Transform parent, Vector3 relativePosition, Quaternion relativeRotation, LayeredTime layeredTime = null) {
            Setup(new Context { clip = animationClip }, slotIndex, time, parent, relativePosition, relativeRotation, layeredTime);
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

            // 操作に必要なコンポーネント追加
            _transposer = AddOrGetComponent<CinemachineTransposer>();
            _transposer.m_XDamping = 0.0f;
            _transposer.m_YDamping = 0.0f;
            _transposer.m_ZDamping = 0.0f;
            
            _composer = AddOrGetComponent<CinemachineComposer>();
            _composer.m_HorizontalDamping = 0.0f;
            _composer.m_VerticalDamping = 0.0f;
            
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
            var slot = CurrentSlot;
            if (slot == null) {
                return;
            }

            VirtualCamera.Follow = slot.follow;
            VirtualCamera.LookAt = slot.lookAt;
        }

        /// <summary>
        /// 仮想カメラの情報を更新
        /// </summary>
        private void UpdateVirtualCamera() {
            var slot = CurrentSlot;
            if (slot == null) {
                return;
            }

            var slotCamera = slot.camera;
            if (slotCamera != null) {
                VirtualCamera.m_Lens.NearClipPlane = slotCamera.nearClipPlane;
                VirtualCamera.m_Lens.FarClipPlane = slotCamera.farClipPlane;
                VirtualCamera.m_Lens.FieldOfView = slotCamera.fieldOfView;
            }

            _transposer.m_FollowOffset = FollowOffset;
            _composer.m_TrackedObjectOffset = LookAtOffset;
            _composer.m_ScreenX = 0.5f + ScreenOffset.x;
            _composer.m_ScreenY = 0.5f + ScreenOffset.y;
        }
    }
}