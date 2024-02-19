using Cinemachine.Utility;
using System;
using System.Linq;
using Cinemachine;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// ランダムにカメラを選ぶ仮想カメラ
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [ExcludeFromPreset]
    [AddComponentMenu("Cinemachine/CinemachineRandomCamera")]
    public class CinemachineBlendListCamera : CinemachineVirtualCameraBase {
        /// <summary>
        /// カメラ情報
        /// </summary>
        [Serializable]
        public class CameraInfo {
            [Tooltip("制御対象のカメラ")]
            public CinemachineVirtualCameraBase m_VirtualCamera;
            [Tooltip("抽選の重み")]
            public float m_Weight = 1.0f;
        }

        [Tooltip("Default object for the camera children to look at (the aim target), if not "
                 + "specified in a child camera.  May be empty if all of the children define targets of their own.")]
        [NoSaveDuringPlay]
        [VcamTargetProperty]
        public Transform m_LookAt;
        [Tooltip("Default object for the camera children wants to move with (the body target), "
                 + "if not specified in a child camera.  May be empty if all of the children define targets of their own.")]
        [NoSaveDuringPlay]
        [VcamTargetProperty]
        public Transform m_Follow;
        [Tooltip("抽選用のカメラ情報")]
        public CameraInfo[] m_CameraInfos;

        private CameraState _cameraState = CameraState.Default;

        /// <summary>デバッグ用の表示情報</summary>
        public override string Description {
            get {
                var vcam = LiveChild;
                if (vcam == null) {
                    return "(none)";
                }

                var sb = CinemachineDebug.SBFromPool();
                sb.Append("[");
                sb.Append(vcam.Name);
                sb.Append("]");
                var text = sb.ToString();
                CinemachineDebug.ReturnToPool(sb);
                return text;
            }
        }
        /// <summary>カメラステート</summary>
        public override CameraState State => _cameraState;
        /// <summary>注視ターゲット</summary>
        public override Transform LookAt {
            get => ResolveLookAt(m_LookAt);
            set => m_LookAt = value;
        }
        /// <summary>Followターゲット</summary>
        public override Transform Follow {
            get => ResolveFollow(m_Follow);
            set => m_Follow = value;
        }
        /// <summary>現在再生中の子カメラ</summary>
        public ICinemachineCamera LiveChild { get; set; }

        /// <summary>
        /// アクティブ時の処理
        /// </summary>
        protected override void OnEnable() {
            base.OnEnable();
            LiveChild = null;
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        private void Reset() {
            m_LookAt = null;
            m_Follow = null;
            m_CameraInfos = Array.Empty<CameraInfo>();
        }

        /// <summary>
        /// Live中のカメラかチェック
        /// </summary>
        public override bool IsLiveChild(ICinemachineCamera vcam, bool dominantChildOnly = false) {
            return vcam == LiveChild;
        }

        /// <summary>
        /// ターゲットオブジェクトのワープ検知
        /// </summary>
        public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta) {
            foreach (var info in m_CameraInfos) {
                var vcam = info.m_VirtualCamera;
                if (vcam == null) {
                    continue;
                }

                vcam.OnTargetObjectWarped(target, positionDelta);
            }

            base.OnTargetObjectWarped(target, positionDelta);
        }

        /// <summary>
        /// 強制Transform更新
        /// </summary>
        public override void ForceCameraPosition(Vector3 pos, Quaternion rot) {
            foreach (var info in m_CameraInfos) {
                var vcam = info.m_VirtualCamera;
                if (vcam == null) {
                    continue;
                }

                vcam.ForceCameraPosition(pos, rot);
            }

            base.ForceCameraPosition(pos, rot);
        }

        /// <summary>
        /// カメラがアクティブになる際の通知
        /// </summary>
        public override void OnTransitionFromCamera(
            ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime) {
            base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
            InvokeOnTransitionInExtensions(fromCam, worldUp, deltaTime);
            
            DrawCamera();
            if (LiveChild != null) {
                LiveChild.UpdateCameraState(worldUp, deltaTime);
            }

            InternalUpdateCameraState(worldUp, deltaTime);
        }

        /// <summary>
        /// カメラステートの更新処理
        /// </summary>
        public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime) {
            // 子カメラの情報をCameraStateに反映
            if (LiveChild != null) {
                _cameraState = LiveChild.State;
            }

            // PostProcessの実行
            InvokePostPipelineStageCallback(this, CinemachineCore.Stage.Finalize, ref _cameraState, deltaTime);
            PreviousStateIsValid = true;
        }

        /// <summary>
        /// カメラの抽選
        /// </summary>
        private void DrawCamera() {
            var totalWeight = m_CameraInfos.Sum(x => x.m_Weight);
            var val = RandomUtil.Range(0.0f, totalWeight - float.Epsilon);
            var foundIndex = m_CameraInfos.Length - 1;
            for (var i = 0; i < m_CameraInfos.Length; i++) {
                var info = m_CameraInfos[i];
                if (val <= info.m_Weight) {
                    foundIndex = i;
                    break;
                }
            }

            // 該当のカメラのみアクティブにする
            for (var i = 0; i < m_CameraInfos.Length; i++) {
                var info = m_CameraInfos[i];
                if (info.m_VirtualCamera == null) {
                    continue;
                }

                var active = foundIndex == i;
                if (info.m_VirtualCamera.gameObject.activeSelf != active) {
                    info.m_VirtualCamera.gameObject.SetActive(active);
                }
            }

            LiveChild = m_CameraInfos[foundIndex].m_VirtualCamera;
        }
    }
}