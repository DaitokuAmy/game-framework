using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.UISystems {
    /// <summary>
    /// TimelineAssetをUIAnimationで再生するためのクラス
    /// </summary>
    public class TimelineUIAnimation : UIAnimation {
        private PlayableDirector _playableDirector;
        private TimelineAsset _timelineAsset;

        /// <summary>再生トータル時間</summary>
        public override float Duration => _timelineAsset != null ? (float)_timelineAsset.duration : 0.0f;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public TimelineUIAnimation(PlayableDirector playableDirector, TimelineAsset timelineAsset)
            : base(playableDirector.gameObject) {
            _playableDirector = playableDirector;

            if (_playableDirector != null) {
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
        protected override void SetTimeInternal(float time) {
            if (_playableDirector != null) {
                _playableDirector.time = time;
                _playableDirector.Evaluate();
            }
        }

        /// <summary>
        /// 再生開始通知
        /// </summary>
        protected override void OnPlayInternal() {
            if (_playableDirector != null) {
                _playableDirector.playableAsset = _timelineAsset;
            }
        }
    }
}