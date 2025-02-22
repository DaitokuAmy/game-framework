using System.Collections;
using System.Collections.Generic;
using ActionSequencer;
using GameFramework.ActorSystems;
using SampleGame.Infrastructure;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace SampleGame.Presentation {
    /// <summary>
    /// ActorAction再生制御用クラス(TriggerState)
    /// </summary>
    public class TriggerStateActorActionResolver : ActorActionResolver<TriggerStateActorAction> {
        private readonly SequenceController _sequenceController;
        private readonly List<SequenceHandle> _sequenceHandles = new();

        private AnimatorControllerPlayable _playable;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playable">AnimatorControllerを制御しているPlayable</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public TriggerStateActorActionResolver(AnimatorControllerPlayable playable, SequenceController sequenceController) {
            _playable = playable;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayActionRoutineInternal(TriggerStateActorAction action, object[] args) {
            if (!_playable.IsValid()) {
                Debug.LogWarning("Invalid trigger state actor action playable.");
                yield break;
            }

            // ステートに移動する
            _playable.SetTrigger(action.triggerPropertyName);
            if (!string.IsNullOrEmpty(action.indexPropertyName)) {
                var index = action.index;
                if (args.Length > 0) {
                    if (args[0] is int overrideIndex) {
                        index = overrideIndex;
                    }
                }

                _playable.SetInteger(action.indexPropertyName, index);
            }

            // シーケンス再生
            foreach (var clip in action.sequenceClips) {
                _sequenceHandles.Add(_sequenceController.Play(clip));
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
                            var time = stateInfo.normalizedTime * stateInfo.length;
                            if (time >= stateInfo.length + action.exitTimeOffset) {
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
                            var time = stateInfo.normalizedTime * stateInfo.length;
                            if (time >= stateInfo.length + action.exitTimeOffset) {
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

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelActionInternal(TriggerStateActorAction action) {
            foreach (var handle in _sequenceHandles) {
                _sequenceController.Stop(handle);
            }

            _sequenceHandles.Clear();
        }
    }
}