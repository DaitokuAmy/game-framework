using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// AnimationClip制御用のVfxComponent
    /// </summary>
    public sealed class AnimationClipVfxComponent : MonoBehaviour, IVfxComponent {
        [SerializeField, Tooltip("再生に使うAnimator")]
        private Animator _animator;
        [SerializeField, Tooltip("再生するクリップ")]
        private AnimationClip _animationClip;

        // 再生に使うPlayableGraph
        private PlayableGraph _playableGraph;
        // AnimationClip用のPlayable
        private AnimationClipPlayable _animationClipPlayable;
        // 再生中か
        private bool _isPlaying;

        /// <inheritdoc/>
        bool IVfxComponent.IsPlaying => _isPlaying;
        
        /// <summary>有効なデータか</summary>
        private bool IsValid => _animator != null && _animationClip != null;

        /// <inheritdoc/>
        void IVfxComponent.Tick(float deltaTime) {
            if (!_playableGraph.IsValid()) {
                return;
            }

            var time = _animationClipPlayable.GetTime() + deltaTime;
            _animationClipPlayable.SetTime(time);
            _playableGraph.Evaluate();

            // Loopでなければ、自動で終わる
            if (!_animationClip.isLooping && time >= _animationClip.length) {
                _isPlaying = false;
            }
        }

        /// <inheritdoc/>
        void IVfxComponent.Play() {
            if (!_playableGraph.IsValid()) {
                return;
            }

            _animationClipPlayable.SetTime(0.0f);
            _playableGraph.Play();
            _isPlaying = true;
        }

        /// <inheritdoc/>
        void IVfxComponent.Stop() {
            if (!_playableGraph.IsValid()) {
                return;
            }

            _animationClipPlayable.SetTime(_animationClip.length);
            _playableGraph.Evaluate();
            _playableGraph.Stop();
            _isPlaying = false;
        }

        /// <inheritdoc/>
        void IVfxComponent.StopImmediate() {
            if (!_playableGraph.IsValid()) {
                return;
            }

            _animationClipPlayable.SetTime(_animationClip.length);
            _playableGraph.Evaluate();
            _playableGraph.Stop();
            _isPlaying = false;
        }

        /// <inheritdoc/>
        void IVfxComponent.SetSpeed(float speed) {
        }

        /// <inheritdoc/>
        void IVfxComponent.SetLodLevel(int level) {
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            if (!IsValid) {
                Debug.LogWarning($"Invalid serialize data. : {gameObject.name}");
                return;
            }

            // Graph構築
            _playableGraph = PlayableGraph.Create($"{nameof(AnimationClipVfxComponent)} - {gameObject.name}");
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var output = AnimationPlayableOutput.Create(_playableGraph, "Output", _animator);
            _animationClipPlayable = AnimationClipPlayable.Create(_playableGraph, _animationClip);
            output.SetSourcePlayable(_animationClipPlayable);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            if (_playableGraph.IsValid()) {
                _playableGraph.Destroy();
            }
        }
    }
}
