using System.Collections;
using ActionSequencer;
using GameFramework.ActorSystems;
using ThirdPersonEngine;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ThirdPersonEngine {
    /// <summary>
    /// ActorAction再生制御用クラス(CrossFadeState)
    /// </summary>
    public class CrossFadeStateActorActionHandler : ActorActionHandler<CrossFadeStateActorAction> {
        private readonly SequenceController _sequenceController;

        private SequenceHandle _sequenceHandle;
        private AnimatorControllerPlayable _playable;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playable">AnimatorControllerを制御しているPlayable</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public CrossFadeStateActorActionHandler(AnimatorControllerPlayable playable, SequenceController sequenceController) {
            _playable = playable;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayRoutineInternal(CrossFadeStateActorAction action) {
            if (!_playable.IsValid()) {
                Debug.LogWarning("Invalid trigger state actor action playable.");
                yield break;
            }

            // ステートに移動する
            _playable.CrossFade(action.firstStateName, action.inBlend);

            // シーケンス再生
            if (action.sequenceClip != null) {
                _sequenceHandle = _sequenceController.Play(action.sequenceClip);
            }

            // 最後のStateが流れ切るのを待つ
            var enteredLast = false;
            while (true) {
                var stateInfo = _playable.GetCurrentAnimatorStateInfo(action.layerIndex);
                var nextStateInfo = _playable.GetNextAnimatorStateInfo(action.layerIndex);

                // Tag判定
                if (!string.IsNullOrEmpty(action.lastTag)) {
                    if (nextStateInfo.fullPathHash != 0) {
                        if (nextStateInfo.IsTag(action.lastTag)) {
                            enteredLast = true;
                        }
                        else if (enteredLast) {
                            break;
                        }
                    }
                    else {
                        if (stateInfo.IsTag(action.lastTag)) {
                            enteredLast = true;
                            if (stateInfo.normalizedTime >= action.exitNormalizedTime) {
                                break;
                            }
                        }
                        else if (enteredLast) {
                            break;
                        }
                    }
                }
                // StateName判定
                else if (!string.IsNullOrEmpty(action.lastStateName)) {
                    if (nextStateInfo.fullPathHash != 0) {
                        if (nextStateInfo.IsName(action.lastStateName)) {
                            enteredLast = true;
                        }
                        else if (enteredLast) {
                            break;
                        }
                    }
                    else {
                        if (stateInfo.IsName(action.lastStateName)) {
                            enteredLast = true;
                            if (stateInfo.normalizedTime >= action.exitNormalizedTime) {
                                break;
                            }
                        }
                        else if (enteredLast) {
                            break;
                        }
                    }
                }

                yield return null;
            }
        }

        /// <inheritdoc/>
        protected override void CancelInternal(CrossFadeStateActorAction action) {
            _sequenceHandle.Dispose();
            _sequenceHandle = default;
        }
    }
}