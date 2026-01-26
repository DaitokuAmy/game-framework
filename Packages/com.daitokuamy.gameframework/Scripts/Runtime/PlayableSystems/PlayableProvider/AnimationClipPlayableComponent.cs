using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimationClipを再生するPlayable用のProvider
    /// </summary>
    public class AnimationClipPlayableComponent : PlayableComponent<AnimationClipPlayable> {
        private AnimationClipPlayable _playable;
        private AnimationClip _clip;

        /// <summary>基礎となるPlayable</summary>
        public override AnimationClipPlayable Playable => _playable;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clip">再生対象のAnimationClip</param>
        public AnimationClipPlayableComponent(AnimationClip clip) {
            _clip = clip;
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph) {
            _playable = AnimationClipPlayable.Create(graph, _clip);
            return _playable;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _clip = null;
        }
    }
}