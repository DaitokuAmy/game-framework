#if USE_SPLINES

using Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// Splineに沿うようにドリーさせるBodyステージのコンポーネント
    /// </summary>
    [DocumentationSorting(DocumentationSortingAttribute.Level.UserRef)]
    [AddComponentMenu("")]
    [SaveDuringPlay]
    public class CinemachineSplineDolly : CinemachineComponentBase {
        [SerializeField, Tooltip("動かすカーブを表すスプライン")]
        private SplineContainer _splineContainer;
        [SerializeField, Tooltip("再生時間")]
        private float _duration = 1.0f;
        [SerializeField, Tooltip("時間カーブ")]
        private AnimationCurve _timeCurve = new(new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, 1.0f));

        private float _currentTime;

        /// <summary>有効判定</summary>
        public override bool IsValid => _splineContainer != null && _splineContainer.Splines.Count > 0 && _duration > float.Epsilon;
        /// <summary>実行ステージ</summary>
        public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Body;

        /// <summary>
        /// CameraStateの加工
        /// </summary>
        public override void MutateCameraState(ref CameraState curState, float deltaTime) {
            if (!IsValid) {
                return;
            }

            _currentTime += deltaTime;
            var rate = _duration > 0.001f ? Mathf.Min(1.0f, _currentTime / _duration) : 1.0f;
            rate = _timeCurve != null && _timeCurve.keys.Length > 1 ? Mathf.Clamp01(_timeCurve.Evaluate(rate)) : rate;
            var position = _splineContainer.Spline.EvaluatePosition(rate);
            if (FollowTarget != null) {
                position = FollowTarget.TransformPoint(position);
            }

            curState.RawPosition = position;
        }

        /// <summary>
        /// カメラ有効時の通知
        /// </summary>
        public override bool OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime, ref CinemachineVirtualCameraBase.TransitionParams transitionParams) {
            _currentTime = 0.0f;
            return true;
        }
    }
}

#endif