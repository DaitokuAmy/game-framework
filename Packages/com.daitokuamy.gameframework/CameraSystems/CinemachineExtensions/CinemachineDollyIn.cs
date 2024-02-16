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
    public class CinemachineDollyIn : CinemachineExtension {
        [Tooltip("制御ステージ"), SaveDuringPlay]
        public CinemachineCore.Stage m_Stage = CinemachineCore.Stage.Aim;
        [Tooltip("ドリーインアニメーション開始用オフセット"), SaveDuringPlay]
        public Vector3 m_StartOffset;
        [Tooltip("再生時間"), SaveDuringPlay]
        public float m_Duration = 1.0f;
        [Tooltip("時間カーブ"), SaveDuringPlay]
        public AnimationCurve m_TimeCurve = new(new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, 1.0f));

        private float _currentTime;

        /// <summary>
        /// 有効になった時の処理
        /// </summary>
        protected override void OnEnable() {
            base.OnEnable();

            _currentTime = 0.0f;
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

            _currentTime += CinemachineCore.DeltaTime;

            // 位置にオフセットを加える
            var rate = m_Duration > 0.001f ? Mathf.Min(1.0f, _currentTime / m_Duration) : 1.0f;
            
            rate = m_TimeCurve != null && m_TimeCurve.keys.Length > 1 ? m_TimeCurve.Evaluate(rate) : rate;
            var offset = Vector3.Lerp(m_StartOffset, Vector3.zero, rate);

            // 位置をずらす
            state.PositionCorrection += state.RawOrientation * offset;
        }
    }
}