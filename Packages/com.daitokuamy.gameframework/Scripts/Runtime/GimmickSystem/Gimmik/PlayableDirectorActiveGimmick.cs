using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFramework.GimmickSystem {
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

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            // 全PlayableDirectorの初期化
            InitializePlayableDirector(_playableDirector);
        }

        /// <inheritdoc/>
        protected override void SetSpeedInternal(float speed) {
            SetSpeedPlayableDirector(_playableDirector, speed);
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(bool immediate) {
            _playableDirector.Play(_activeTimelineAsset);
            _playableDirector.time = 0.0f;
            if (immediate) {
                _playableDirector.time = _playableDirector.duration;
                _playableDirector.Evaluate();
            }
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal(bool immediate) {
            _playableDirector.Play(_inactiveTimelineAsset);
            _playableDirector.time = 0.0f;
            if (immediate) {
                _playableDirector.time = _playableDirector.duration;
                _playableDirector.Evaluate();
            }
        }

        /// <summary>
        /// PlayableDirectorの初期化
        /// </summary>
        private void InitializePlayableDirector(PlayableDirector playableDirector) {
            if (playableDirector == null) {
                return;
            }

            playableDirector.playOnAwake = false;
            playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            playableDirector.extrapolationMode = DirectorWrapMode.Hold;
            playableDirector.Play(_activeTimelineAsset);
            playableDirector.time = 0.0f;
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
