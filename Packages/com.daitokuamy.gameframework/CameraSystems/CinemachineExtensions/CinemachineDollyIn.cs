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
        [SerializeField, Tooltip("ドリーインアニメーション開始用オフセット")]
        private Vector3 _startOffset;
        [SerializeField, Tooltip("再生時間")]
        private float _duration = 1.0f;
        [SerializeField, Tooltip("時間カーブ")]
        private AnimationCurve _timeCurve = new(new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, 1.0f));

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
            if (stage != CinemachineCore.Stage.Aim) {
                return;
            }

            if (_currentTime > _duration) {
                return;
            }

            _currentTime += CinemachineCore.DeltaTime;

            // 位置にオフセットを加える
            var rate = _duration > 0.001f ? Mathf.Min(1.0f, _currentTime / _duration) : 1.0f;
            
            rate = _timeCurve != null && _timeCurve.keys.Length > 1 ? _timeCurve.Evaluate(rate) : rate;
            var offset = Vector3.Lerp(_startOffset, Vector3.zero, rate);

            // 位置をずらす
            state.PositionCorrection += state.RawOrientation * offset;
        }
    }
}