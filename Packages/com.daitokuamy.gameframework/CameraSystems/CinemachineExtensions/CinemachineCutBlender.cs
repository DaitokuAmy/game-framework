using Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// Splineに沿うようにドリーさせるBodyステージのコンポーネント
    /// </summary>
    [SaveDuringPlay]
    [AddComponentMenu("")]
    [DocumentationSorting(DocumentationSortingAttribute.Level.UserRef)]
    [ExecuteAlways]
    public class CinemachineCutBlender : CinemachineExtension {
        [Tooltip("制御ステージ")]
        public CinemachineCore.Stage m_Stage = CinemachineCore.Stage.Aim;
        [Tooltip("ブレンドに使う割合"), Range(0.0f, 1.0f)]
        public float m_BlendRate = 0.2f;
        [Tooltip("再生時間")]
        public float m_Duration = 1.0f;
        [Tooltip("時間カーブ")]
        public AnimationCurve m_TimeCurve = new(new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, 1.0f));

        private float _currentTime;
        private Vector3? _fromPosition;

        /// <summary>
        /// 有効になった時の処理
        /// </summary>
        protected override void OnEnable() {
            base.OnEnable();

            _currentTime = 0.0f;
            var brain = CinemachineCore.Instance.FindPotentialTargetBrain(VirtualCamera);
            _fromPosition = brain != null ? brain.transform.position : null;
        }

        /// <summary>
        /// 処理の上書き
        /// </summary>
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
            if (stage != m_Stage) {
                return;
            }

            if (_currentTime > m_Duration) {
                return;
            }

            if (_fromPosition == null) {
                return;
            }

            _currentTime += CinemachineCore.DeltaTime;

            // 元の位置からブレンドする
            var rate = m_Duration > 0.001f ? Mathf.Min(1.0f, _currentTime / m_Duration) : 1.0f;
            rate = m_TimeCurve != null && m_TimeCurve.keys.Length > 1 ? m_TimeCurve.Evaluate(rate) : rate;
            var finalPosition = Vector3.Lerp(_fromPosition.Value, state.FinalPosition, (1.0f - m_BlendRate) + rate * m_BlendRate);

            // 位置をずらす
            state.PositionCorrection = finalPosition - state.RawPosition;
        }
    }
}