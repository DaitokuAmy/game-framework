using Cinemachine;
using UnityEngine;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// 手振れ用のCinemachineExtension
    /// </summary>
    [SaveDuringPlay]
    [AddComponentMenu("")]
    [DocumentationSorting(DocumentationSortingAttribute.Level.UserRef)]
    [ExecuteAlways]
    public class CinemachineJitter : CinemachineExtension {
        [Tooltip("座標の周波数"), SaveDuringPlay]
        public float m_PositionFrequency = 0.05f;
        [Tooltip("回転の周波数"), SaveDuringPlay]
        public float m_RotationFrequency = 0.05f;

        [Tooltip("座標の移動量"), SaveDuringPlay]
        public float m_PositionAmount = 0.1f;
        [Tooltip("回転の移動量"), SaveDuringPlay]
        public float m_RotationAmount = 1.5f;

        [Tooltip("座標のウェイト"), SaveDuringPlay]
        public Vector3 m_PositionComponents = Vector3.one;
        [Tooltip("回転のウェイト"), SaveDuringPlay]
        public Vector3 m_RotationComponents = new(1, 1, 0);

        [Tooltip("座標の音程"), SaveDuringPlay]
        public int m_PositionOctave = 3;
        [Tooltip("回転の音程"), SaveDuringPlay]
        public int m_RotationOctave = 3;

        private float _timePosition;
        private float _timeRotation;
        private Vector2[] _noiseVectors;

        /// <summary>
        /// 有効になった時の処理
        /// </summary>
        protected override void OnEnable() {
            base.OnEnable();

            _timePosition = 0.0f;
            _timeRotation = 0.0f;
            _noiseVectors = new Vector2[6];

            for (var i = 0; i < 6; i++) {
                var theta = Random.value * Mathf.PI * 2;
                _noiseVectors[i].Set(Mathf.Cos(theta), Mathf.Sin(theta));
            }
        }

        /// <summary>
        /// 処理の上書き
        /// </summary>
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
            if (stage != CinemachineCore.Stage.Aim) {
                return;
            }

            _timePosition += deltaTime * m_PositionFrequency;
            _timeRotation += deltaTime * m_RotationFrequency;

            if (m_PositionAmount * m_PositionAmount > float.Epsilon) {
                var offsetPos = new Vector3(
                    Fbm(_noiseVectors[0] * _timePosition, m_PositionOctave),
                    Fbm(_noiseVectors[1] * _timePosition, m_PositionOctave),
                    Fbm(_noiseVectors[2] * _timePosition, m_PositionOctave)
                );
                offsetPos = Vector3.Scale(offsetPos, m_PositionComponents) * (m_PositionAmount * 2);
                state.RawPosition += state.RawOrientation * offsetPos;
            }

            if (m_RotationAmount * m_RotationAmount > float.Epsilon) {
                var offsetAngles = new Vector3(
                    Fbm(_noiseVectors[3] * _timeRotation, m_RotationOctave),
                    Fbm(_noiseVectors[4] * _timeRotation, m_RotationOctave),
                    Fbm(_noiseVectors[5] * _timeRotation, m_RotationOctave)
                );
                offsetAngles = Vector3.Scale(offsetAngles, m_RotationComponents) * (m_RotationAmount * 2);
                state.RawOrientation = state.RawOrientation * Quaternion.Euler(offsetAngles);
            }
        }

        /// <summary>
        /// 移動量計算
        /// </summary>
        private float Fbm(Vector2 coord, int octave) {
            var f = 0.0f;
            var w = 1.0f;
            for (var i = 0; i < octave; i++) {
                f += w * Mathf.PerlinNoise(coord.x, coord.y) * 0.5f;
                coord *= 2;
                w *= 0.5f;
            }

            return f;
        }
    }
}