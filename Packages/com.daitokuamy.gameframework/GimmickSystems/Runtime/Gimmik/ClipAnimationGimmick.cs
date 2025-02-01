using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// AnimationClipを使ったアニメーション再生ギミック
    /// </summary>
    public class ClipAnimationGimmick : AnimationGimmick {
        [SerializeField, Tooltip("再生させるAnimator")]
        private Animator _animator;
        [SerializeField, Tooltip("再生に使うAnimationClip")]
        private AnimationClip _animationClip;
        [SerializeField, Tooltip("再生に使用するAvatarMask")]
        private AvatarMask _avatarMask;

        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _layerMixerPlayable;
        private AnimationClipPlayable _animationClipPlayable;

        // トータル時間
        public override float Duration => _animationClip != null ? _animationClip.length : 0.0f;
        // ループ再生するか
        public override bool IsLooping => _animationClip != null && _animationClip.isLooping;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            _graph = PlayableGraph.Create($"AnimationGimmick_{_animationClip}");
            _layerMixerPlayable = AnimationLayerMixerPlayable.Create(_graph, 1, true);
            _animationClipPlayable = AnimationClipPlayable.Create(_graph, _animationClip);
            _layerMixerPlayable.ConnectInput(0, _animationClipPlayable, 0);
            if (_avatarMask != null) {
                _layerMixerPlayable.SetLayerMaskFromAvatarMask(0, _avatarMask);
            }

            _layerMixerPlayable.SetInputWeight(0, 1.0f);
            _layerMixerPlayable.SetLayerAdditive(0, false);
            var output = AnimationPlayableOutput.Create(_graph, "output", _animator);
            output.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
            output.SetSourcePlayable(_layerMixerPlayable);
            _graph.Play();
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            if (_graph.IsValid()) {
                _graph.Destroy();
            }

            base.DisposeInternal();
        }

        /// <summary>
        /// 再生状態の反映
        /// </summary>
        protected override void Evaluate(float time) {
            _animationClipPlayable.SetTime(time);
            _graph.Evaluate(0.0f);
        }
    }
}