using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimatorControllerを再生するPlayable用のProvider
    /// </summary>
    public class AnimatorControllerPlayableComponent : PlayableComponent<AnimatorControllerPlayable> {
        private AnimatorControllerPlayable _playable;
        private RuntimeAnimatorController _controller;

        /// <summary>基礎となるPlayable</summary>
        public override AnimatorControllerPlayable Playable => _playable;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="controller">再生対象のController</param>
        /// <param name="autoDispose">自動廃棄するか</param>
        public AnimatorControllerPlayableComponent(RuntimeAnimatorController controller, bool autoDispose)
            : base(autoDispose) {
            _controller = controller;
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph) {
            _playable = AnimatorControllerPlayable.Create(graph, _controller);
            return _playable;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _controller = null;
        }
    }
}