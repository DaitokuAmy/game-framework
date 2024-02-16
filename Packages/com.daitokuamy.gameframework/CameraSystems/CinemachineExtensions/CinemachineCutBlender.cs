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
        [SerializeField, Tooltip("制御ステージ")]
        private CinemachineCore.Stage _stage = CinemachineCore.Stage.Aim;
        [SerializeField, Tooltip("ブレンドに使う割合"), Range(0.0f, 1.0f)]
        private float _blendRate = 0.2f;
        [SerializeField, Tooltip("再生時間")]
        private float _duration = 1.0f;
        [SerializeField, Tooltip("時間カーブ")]
        private AnimationCurve _timeCurve = new(new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, 1.0f));

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
            if (stage != _stage) {
                return;
            }

            if (_currentTime > _duration) {
                return;
            }

            if (_fromPosition == null) {
                return;
            }

            _currentTime += CinemachineCore.DeltaTime;

            // 元の位置からブレンドする
            var rate = _duration > 0.001f ? Mathf.Min(1.0f, _currentTime / _duration) : 1.0f;
            rate = _timeCurve != null && _timeCurve.keys.Length > 1 ? _timeCurve.Evaluate(rate) : rate;
            var finalPosition = Vector3.Lerp(_fromPosition.Value, state.FinalPosition, (1.0f - _blendRate) + rate * _blendRate);

            // 位置をずらす
            state.PositionCorrection = finalPosition - state.RawPosition;
            Debug.Log($"_currenTime:{_currentTime}, deltaTime:{deltaTime}");
        }
    }
}