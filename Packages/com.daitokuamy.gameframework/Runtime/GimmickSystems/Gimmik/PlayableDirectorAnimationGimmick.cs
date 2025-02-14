using UnityEngine;
using UnityEngine.Playables;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// PlayableDirectorを使ったアニメーション再生ギミック
    /// </summary>
    public class PlayableDirectorAnimationGimmick : AnimationGimmick {
        [SerializeField, Tooltip("再生させるPlayableDirector")]
        private PlayableDirector _playableDirector;

        // トータル時間
        public override float Duration => _playableDirector != null ? (float)_playableDirector.duration : 0.0f;
        // ループ再生するか
        public override bool IsLooping => _playableDirector != null && _playableDirector.extrapolationMode == DirectorWrapMode.Loop;

        /// <summary>
        /// 初期化処理
        /// </summary>
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

        /// <summary>
        /// 再生状態の反映
        /// </summary>
        protected override void Evaluate(float time) {
            if (_playableDirector != null) {
                _playableDirector.time = time;
                _playableDirector.Evaluate();
            }
        }
    }
}