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

        private CinemachineFollow _follow;
        private CinemachineRotationComposer _composer;

        private int _slotIndex;

        /// <summary>注視点オフセット</summary>
        public Vector3 LookAtOffset { get; set; }
        /// <summary>フォローオフセット</summary>
        public Vector3 FollowOffset { get; set; }
        /// <summary>スクリーンオフセット(中央:0,0)</summary>
        public Vector2 ScreenOffset { get; set; }

        /// <summary>現在のSlot</summary>
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
            Setup(new Context { clip = animationClip }, 0, 0.0f, parent, relativePosition, relativeRotation, layeredTime);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            T AddOrGetComponent<T>(CinemachineCore.Stage stage)
                where T : CinemachineComponentBase {
                var bodyComponent = VirtualCamera.GetCinemachineComponent(stage);
                if (bodyComponent != null) {
                    if (bodyComponent is T component) {
                        return component;
                    }

                    Destroy(bodyComponent);
                }

                return VirtualCamera.gameObject.AddComponent<T>();
            }

            // 操作に必要なコンポーネント追加
            _follow = AddOrGetComponent<CinemachineFollow>(CinemachineCore.Stage.Body);
            _follow.TrackerSettings.PositionDamping = Vector3.zero;
            _follow.TrackerSettings.RotationDamping = Vector3.zero;
            _follow.TrackerSettings.QuaternionDamping = 0.0f;

            _composer = AddOrGetComponent<CinemachineRotationComposer>(CinemachineCore.Stage.Aim);
            _composer.Damping = Vector2.zero;

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
                VirtualCamera.Lens.NearClipPlane = slotCamera.nearClipPlane;
                VirtualCamera.Lens.FarClipPlane = slotCamera.farClipPlane;
                VirtualCamera.Lens.FieldOfView = slotCamera.fieldOfView;

                if (slotCamera.orthographic) {
                    VirtualCamera.Lens.OrthographicSize = slotCamera.orthographicSize;
                    VirtualCamera.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
                }
                else if (slotCamera.usePhysicalProperties) {
                    VirtualCamera.Lens.FieldOfView = slotCamera.fieldOfView;
                    VirtualCamera.Lens.PhysicalProperties.FocusDistance = slotCamera.focusDistance;
                    VirtualCamera.Lens.PhysicalProperties.SensorSize = slotCamera.sensorSize;
                    VirtualCamera.Lens.PhysicalProperties.LensShift = slotCamera.lensShift;
                    VirtualCamera.Lens.PhysicalProperties.BarrelClipping = slotCamera.barrelClipping;
                    VirtualCamera.Lens.PhysicalProperties.Curvature = slotCamera.curvature;
                    VirtualCamera.Lens.PhysicalProperties.Anamorphism = slotCamera.anamorphism;
                    VirtualCamera.Lens.PhysicalProperties.Aperture = slotCamera.aperture;
                    VirtualCamera.Lens.PhysicalProperties.ShutterSpeed = slotCamera.shutterSpeed;
                    VirtualCamera.Lens.PhysicalProperties.Iso = slotCamera.iso;
                    VirtualCamera.Lens.PhysicalProperties.BladeCount = slotCamera.bladeCount;
                    VirtualCamera.Lens.PhysicalProperties.GateFit = slotCamera.gateFit;
                    VirtualCamera.Lens.ModeOverride = LensSettings.OverrideModes.Physical;
                }
                else {
                    VirtualCamera.Lens.FieldOfView = slotCamera.fieldOfView;
                    VirtualCamera.Lens.ModeOverride = LensSettings.OverrideModes.None;
                }
            }

            _follow.FollowOffset = FollowOffset;
            _composer.TargetOffset = LookAtOffset;
            _composer.Composition.ScreenPosition = new Vector2(0.5f, 0.5f) + ScreenOffset;
        }
    }
}