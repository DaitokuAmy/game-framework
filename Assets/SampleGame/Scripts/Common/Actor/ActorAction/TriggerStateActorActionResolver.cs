using System.Collections;
using System.Collections.Generic;
using ActionSequencer;
using GameFramework.ActorSystems;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace SampleGame {
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
                _playable.SetInteger(action.indexPropertyName, action.index);
            }

            // シーケンス再生
            foreach (var clip in action.sequenceClips) {
                _sequenceHandles.Add(_sequenceController.Play(clip));
            }

            // 最後のStateが流れ切るのを待つ
            var enteredLastState = false;
            while (true) {
                var stateInfo = _playable.GetCurrentAnimatorStateInfo(action.layerIndex);

                // LastStateに入っているか
                if (stateInfo.IsName(action.lastStateName)) {
                    enteredLastState = true;
                    if (stateInfo.normalizedTime >= 1.0f) {
                        break;
                    }
                }
                // 既にLastStateに入った状態から移ったか
                else if (enteredLastState) {
                    break;
                }
                
                yield return null;
            }
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelActionInternal() {
            foreach (var handle in _sequenceHandles) {
                _sequenceController.Stop(handle);
            }

            _sequenceHandles.Clear();
        }
    }
}