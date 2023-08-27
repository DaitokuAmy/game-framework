using System.Collections;
using System.Collections.Generic;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;

namespace SampleGame {
    /// <summary>
    /// ActorAction再生制御用クラス(AnimatorController)
    /// </summary>
    public class ControllerActorActionResolver : ActorActionResolver<ControllerActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;

        // 再生中のSequenceHandle
        private readonly List<SequenceHandle> _sequenceHandles = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">モーション制御に使うMotionController</param>
        /// <param name="sequenceController">SequenceClip再生用Controller</param>
        public ControllerActorActionResolver(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayActionRoutineInternal(ControllerActorAction action, object[] args) {
            // クリップを再生する
            var provider = _motionHandle.Change(action.controller, action.inBlend);
            
            // シーケンス再生
            foreach (var clip in action.sequenceClips) {
                _sequenceHandles.Add(_sequenceController.Play(clip));
            }

            // 最後のStateが流れ切るのを待つ
            var controllerPlayable = provider.Playable;
            var enteredLastState = false;
            while (true) {
                var stateInfo = controllerPlayable.GetCurrentAnimatorStateInfo(0);

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
            
            _sequenceHandles.Clear();
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