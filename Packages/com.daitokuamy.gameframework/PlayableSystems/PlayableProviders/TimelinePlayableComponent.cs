using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimatorControllerを再生するPlayable用のProvider
    /// </summary>
    public class TimelinePlayableComponent : PlayableComponent<ScriptPlayable<TimelinePlayable>> {
        private ScriptPlayable<TimelinePlayable> _playable;
        private Animator _animator;
        private TimelineAsset _timelineAsset;

        /// <summary>基礎となるPlayable</summary>
        public override ScriptPlayable<TimelinePlayable> Playable => _playable;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="animator">再生対象のAnimator</param>
        /// <param name="timelineAsset">再生対象のTimelineAsset</param>
        /// <param name="autoDispose">自動廃棄するか</param>
        public TimelinePlayableComponent(Animator animator, TimelineAsset timelineAsset, bool autoDispose)
            : base(autoDispose) {
            _animator = animator;
            _timelineAsset = timelineAsset;
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph) {
            var tracks = _timelineAsset.GetOutputTracks()
                .OfType<AnimationTrack>();
            _playable = TimelinePlayable.Create(graph, tracks, _animator.gameObject, true, false);
            _playable.SetDuration(_timelineAsset.duration);
            return _playable;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _animator = null;
            _timelineAsset = null;
        }
    }
}