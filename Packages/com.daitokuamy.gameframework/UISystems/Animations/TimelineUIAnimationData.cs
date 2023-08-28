using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.UISystems {    
    /// <summary>
    /// UIAnimationをScriptableObjectベースで制御するための基底
    /// </summary>
    public class TimelineUIAnimationData : UIAnimationData<TimelineUIAnimationData, TimelineUIAnimationData.Animation> {
        /// <summary>
        /// 再生実行用のAnimationクラス
        /// </summary>
        public new class Animation : UIAnimationData<TimelineUIAnimationData, Animation>.Animation {
            private PlayableDirector _playableDirector;
            
            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal() {
                _playableDirector = RootObject.GetComponent<PlayableDirector>();

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
                    _playableDirector.playableAsset = Data.timelineAsset;
                }
            }
        }

        [Tooltip("再生するTimelineAsset")]
        public TimelineAsset timelineAsset;

        /// <summary>再生トータル時間</summary>
        protected override float Duration => timelineAsset != null ? (float)timelineAsset.duration : 0.0f;
    }
}