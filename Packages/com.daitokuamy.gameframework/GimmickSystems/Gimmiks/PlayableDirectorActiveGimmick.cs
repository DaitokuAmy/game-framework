using UnityEngine;
using UnityEngine.Playables;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// PlayableDirectorのアクティブコントロールをするGimmick
    /// </summary>
    public class PlayableDirectorActiveGimmick : ActiveGimmick {
        [SerializeField, Tooltip("Active制御する対象")]
        private PlayableDirector[] _activeTargets;
        [SerializeField, Tooltip("Inactive制御する対象")]
        private PlayableDirector[] _inactiveTargets;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            // 全PlayableDirectorの初期化
            foreach (var target in _activeTargets) {
                InitializePlayableDirector(target);
            }
            
            foreach (var target in _inactiveTargets) {
                InitializePlayableDirector(target);
            }
        }

        /// <summary>
        /// 速度の変更
        /// </summary>
        protected override void SetSpeedInternal(float speed) {
            foreach (var target in _activeTargets) {
                SetSpeedPlayableDirector(target, speed);
            }
        }

        /// <summary>
        /// アクティブ化処理
        /// </summary>
        protected override void ActivateInternal() {
            foreach (var target in _inactiveTargets) {
                if (target == null) {
                    continue;
                }

                target.Stop();
            }
            
            foreach (var target in _activeTargets) {
                if (target == null) {
                    continue;
                }

                target.Play();
            }
        }

        /// <summary>
        /// 非アクティブ化処理
        /// </summary>
        protected override void DeactivateInternal() {
            foreach (var target in _activeTargets) {
                if (target == null) {
                    continue;
                }

                target.Stop();
            }
            
            foreach (var target in _inactiveTargets) {
                if (target == null) {
                    continue;
                }

                target.Play();
            }
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