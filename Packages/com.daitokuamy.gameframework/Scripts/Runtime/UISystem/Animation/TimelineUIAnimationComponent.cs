using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.UISystem {    
    /// <summary>
    /// UIAnimationを再生するためのComponent
    /// </summary>
    public class TimelineUIAnimationComponent : UIAnimationComponent {
        [SerializeField, Tooltip("再生に使うPlayableDirector")]
        private PlayableDirector _playableDirector;
        [SerializeField, Tooltip("再生するTimelineAsset(nullで初期値)")]
        private TimelineAsset _timelineAsset;

        /// <summary>トータル時間</summary>
        public override float Duration => _playableDirector != null ? (float)_playableDirector.duration : 0.0f;
        
        /// <inheritdoc/>
        protected override void InitializeInternal() {
            if (_playableDirector != null) {
                _playableDirector.time = 0.0;
                _playableDirector.playOnAwake = false;
                _playableDirector.initialTime = 0.0;
                _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
                _playableDirector.extrapolationMode = DirectorWrapMode.Hold;
            }
        }
        
        /// <inheritdoc/>
        protected override void SetTimeInternal(float time) {
            if (_playableDirector != null) {
                _playableDirector.time = time;
                _playableDirector.Evaluate();
            }
        }

        /// <inheritdoc/>
        protected override void OnPlayInternal() {
            if (_playableDirector != null) {
                _playableDirector.Play(_timelineAsset);
                _playableDirector.Evaluate();
            }
        }
    }
}
