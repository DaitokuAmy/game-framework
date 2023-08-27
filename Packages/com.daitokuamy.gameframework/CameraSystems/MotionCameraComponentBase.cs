using Cinemachine;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// モーション制御用カメラコンポーネントの規定
    /// </summary>
    public abstract class MotionCameraComponentBase : SerializedCameraComponent<CinemachineVirtualCamera> {
        [SerializeField, Tooltip("再生起点にするためのTransform")]
        private Transform _rootTransform;
        [SerializeField, Tooltip("モーション再生用のAnimator")]
        private Animator _animator;

        // アニメーション再生用
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationClipPlayable _playable;
        
        private float _currentTime;
        private LayeredTime _layeredTime;

        private Transform _parentTransform;
        private Vector3 _relativePosition;
        private Quaternion _relativeRotation;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="time">再生時間</param>
        /// <param name="animationClip">再生用クリップ</param>
        /// <param name="parent">追従対象のTransform</param>
        /// <param name="relativePosition">相対座標</param>
        /// <param name="relativeRotation">相対向き</param>
        /// <param name="layeredTime">時間コントロール</param>
        protected void SetupInternal(float time, AnimationClip animationClip, Transform parent, Vector3 relativePosition, Quaternion relativeRotation, LayeredTime layeredTime) {
            _layeredTime = layeredTime;
            _currentTime = time;

            SetupRoot(parent, relativePosition, relativeRotation);
            SetupPlayable(animationClip);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // Graphの構築
            _graph = PlayableGraph.Create($"[{nameof(LookAtMotionCameraComponent)}]");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            _output = AnimationPlayableOutput.Create(_graph, "output", _animator);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            if (_playable.IsValid()) {
                _playable.Destroy();
            }

            if (_graph.IsValid()) {
                _graph.Destroy();
            }
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            ApplyRootTransform();
            UpdateGraph(deltaTime);
        }

        /// <summary>
        /// Rootの初期化
        /// </summary>
        private void SetupRoot(Transform parent, Vector3 relativePosition, Quaternion relativeRotation) {
            if (_rootTransform == null) {
                return;
            }

            _parentTransform = parent;
            _relativePosition = relativePosition;
            _relativeRotation = relativeRotation;
            ApplyRootTransform();
        }

        /// <summary>
        /// Playableの初期化
        /// </summary>
        private void SetupPlayable(AnimationClip clip) {
            if (_playable.IsValid()) {
                _playable.Destroy();
            }

            if (clip == null) {
                Debug.LogWarning("Not found motion camera clip");
                return;
            }

            _playable = AnimationClipPlayable.Create(_graph, clip);
            _playable.SetDuration(clip.length);
            _output.SetSourcePlayable(_playable);
        }

        /// <summary>
        /// PlayableGraphの更新
        /// </summary>
        private void UpdateGraph(float deltaTime) {
            if (!_playable.IsValid()) {
                return;
            }
            
            _currentTime += _layeredTime?.DeltaTime ?? deltaTime;
            var duration = (float)_playable.GetDuration();
            _playable.SetTime(Mathf.Min(_currentTime, duration - 0.0001f));
            _graph.Evaluate();
        }

        /// <summary>
        /// RootTransformの更新
        /// </summary>
        private void ApplyRootTransform() {
            if (_rootTransform == null) {
                return;
            }

            var pos = _relativePosition;
            var rot = _relativeRotation;
            if (_parentTransform != null) {
                pos = _parentTransform.TransformPoint(pos);
                rot = _parentTransform.rotation * _relativeRotation;
            }
            
            _rootTransform.SetPositionAndRotation(pos, rot);
        }
    }
}