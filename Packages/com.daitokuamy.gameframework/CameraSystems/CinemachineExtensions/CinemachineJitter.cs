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
        [SerializeField, Tooltip("座標の周波数")]
        private float _positionFrequency = 0.2f;
        [SerializeField, Tooltip("回転の周波数")]
        private float _rotationFrequency = 0.2f;

        [SerializeField, Tooltip("座標の移動量")]
        private float _positionAmount = 1.0f;
        [SerializeField, Tooltip("回転の移動量")]
        private float _rotationAmount = 30.0f;

        [SerializeField, Tooltip("座標のウェイト")]
        private Vector3 _positionComponents = Vector3.one;
        [SerializeField, Tooltip("回転のウェイト")]
        private Vector3 _rotationComponents = new(1, 1, 0);

        [SerializeField, Tooltip("座標の音程")]
        private int _positionOctave = 3;
        [SerializeField, Tooltip("回転の音程")]
        private int _rotationOctave = 3;

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

            _timePosition += deltaTime * _positionFrequency;
            _timeRotation += deltaTime * _rotationFrequency;

            if (_positionAmount * _positionAmount > float.Epsilon) {
                var offsetPos = new Vector3(
                    Fbm(_noiseVectors[0] * _timePosition, _positionOctave),
                    Fbm(_noiseVectors[1] * _timePosition, _positionOctave),
                    Fbm(_noiseVectors[2] * _timePosition, _positionOctave)
                );
                offsetPos = Vector3.Scale(offsetPos, _positionComponents) * (_positionAmount * 2);
                state.RawPosition += state.RawOrientation * offsetPos;
            }

            if (_rotationAmount * _rotationAmount > float.Epsilon) {
                var offsetAngles = new Vector3(
                    Fbm(_noiseVectors[3] * _timeRotation, _rotationOctave),
                    Fbm(_noiseVectors[4] * _timeRotation, _rotationOctave),
                    Fbm(_noiseVectors[5] * _timeRotation, _rotationOctave)
                );
                offsetAngles = Vector3.Scale(offsetAngles, _rotationComponents) * (_rotationAmount * 2);
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