using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.UISystems {
    /// <summary>
    /// TimelineAssetをUIAnimationで再生するためのクラス
    /// </summary>
    public class TimelineUIAnimation : IUIAnimation {
        private readonly PlayableDirector _playableDirector;
        private TimelineAsset _timelineAsset;

        /// <summary>再生トータル時間</summary>
        public float Duration => _timelineAsset != null ? (float)_timelineAsset.duration : 0.0f;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public TimelineUIAnimation(PlayableDirector playableDirector, TimelineAsset timelineAsset) {
            _playableDirector = playableDirector;
            _timelineAsset = timelineAsset;

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
        void IUIAnimation.SetTime(float time) {
            if (_playableDirector != null) {
                _playableDirector.time = time;
                _playableDirector.Evaluate();
            }
        }

        /// <summary>
        /// 再生開始通知
        /// </summary>
        void IUIAnimation.OnPlay() {
            if (_playableDirector != null) {
                _playableDirector.Stop();
                _playableDirector.Play(_timelineAsset);
                _playableDirector.Evaluate();
            }
        }

        /// <summary>
        /// タイムラインアセットの変更
        /// </summary>
        public void ChangeTimelineAsset(TimelineAsset timelineAsset) {
            _timelineAsset = timelineAsset;
        }
    }
}