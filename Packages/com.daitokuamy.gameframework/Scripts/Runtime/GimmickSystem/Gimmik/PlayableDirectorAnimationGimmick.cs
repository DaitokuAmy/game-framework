using UnityEngine;
using UnityEngine.Playables;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// PlayableDirectorを使ったアニメーション再生ギミック
    /// </summary>
    public class PlayableDirectorAnimationGimmick : AnimationGimmick {
        [SerializeField, Tooltip("再生させるPlayableDirector")]
        private PlayableDirector _playableDirector;

        /// <summary>トータル時間</summary>
        public override float Duration => _playableDirector != null ? (float)_playableDirector.duration : 0.0f;
        /// <summary>ループ再生するか</summary>
        public override bool IsLooping => _playableDirector != null && _playableDirector.extrapolationMode == DirectorWrapMode.Loop;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            if (_playableDirector != null) {
                _playableDirector.time = 0.0;
                _playableDirector.playOnAwake = false;
                _playableDirector.initialTime = 0.0;
                _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
                _playableDirector.extrapolationMode = DirectorWrapMode.Hold;
                _playableDirector.Play();
            }
        }

        /// <inheritdoc/>
        protected override void Evaluate(float time) {
            if (_playableDirector != null) {
                _playableDirector.time = time;
                _playableDirector.Evaluate();
            }
        }
    }
}
