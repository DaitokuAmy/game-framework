using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// PlayableDirectorのアクティブコントロールをするGimmick
    /// </summary>
    public class PlayableDirectorActiveGimmick : ActiveGimmick {
        [SerializeField, Tooltip("制御に使うPlayableDirector")]
        private PlayableDirector _playableDirector;
        [SerializeField, Tooltip("Active時に流すAsset")]
        private TimelineAsset _activeTimelineAsset;
        [SerializeField, Tooltip("Inactive時に流すAsset")]
        private TimelineAsset _inactiveTimelineAsset;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            // 全PlayableDirectorの初期化
            InitializePlayableDirector(_playableDirector);
        }

        /// <summary>
        /// 速度の変更
        /// </summary>
        protected override void SetSpeedInternal(float speed) {
            SetSpeedPlayableDirector(_playableDirector, speed);
        }

        /// <summary>
        /// アクティブ化処理
        /// </summary>
        protected override void ActivateInternal() {
            _playableDirector.Play(_activeTimelineAsset);
        }

        /// <summary>
        /// 非アクティブ化処理
        /// </summary>
        protected override void DeactivateInternal() {
            _playableDirector.Play(_inactiveTimelineAsset);
        }

        /// <summary>
        /// PlayableDirectorの初期化
        /// </summary>
        private void InitializePlayableDirector(PlayableDirector playableDirector) {
            if (playableDirector == null) {
                return;
            }
            
            playableDirector.time = 0.0f;
            playableDirector.playOnAwake = false;
            playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            playableDirector.extrapolationMode = DirectorWrapMode.Hold;
            playableDirector.Play(_activeTimelineAsset);
            playableDirector.Stop();
        }

        /// <summary>
        /// PlayableDirectorの速度変更
        /// </summary>
        private void SetSpeedPlayableDirector(PlayableDirector playableDirector, float speed) {
            if (playableDirector == null) {
                return;
            }

            if (playableDirector.playableGraph.IsValid()) {
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
            }
        }
    }
}