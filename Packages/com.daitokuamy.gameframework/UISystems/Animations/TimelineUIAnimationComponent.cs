using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.UISystems {    
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
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            if (_playableDirector != null) {
                _playableDirector.playableAsset = _timelineAsset ? _timelineAsset : _playableDirector.playableAsset;
                _playableDirector.time = 0.0;
                _playableDirector.playOnAwake = false;
                _playableDirector.initialTime = 0.0;
                _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
                _playableDirector.extrapolationMode = DirectorWrapMode.Hold;
            }
        }
        
        /// <summary>
        /// 時間の設定
        /// </summary>
        /// <param name="time">現在時間</param>
        protected override void SetTimeInternal(float time) {
            if (_playableDirector != null) {
                _playableDirector.time = time;
                _playableDirector.Evaluate();
            }
        }
    }
}