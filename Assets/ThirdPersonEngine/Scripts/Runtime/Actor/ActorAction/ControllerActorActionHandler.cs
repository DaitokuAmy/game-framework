using System.Collections;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using ThirdPersonEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// ActorAction再生制御用クラス(AnimatorController)
    /// </summary>
    public class ControllerActorActionHandler : ActorActionHandler<ControllerActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;

        private SequenceHandle _sequenceHandle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">モーション制御に使うMotionController</param>
        /// <param name="sequenceController">SequenceClip再生用Controller</param>
        public ControllerActorActionHandler(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayRoutineInternal(ControllerActorAction action) {
            // クリップを再生する
            var provider = _motionHandle.Change(action.controller, action.inBlend);
            
            // シーケンス再生
            if (action.sequenceClip != null) {
                _sequenceHandle = _sequenceController.Play(action.sequenceClip);
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
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelInternal(ControllerActorAction action) {
            _sequenceHandle.Dispose();
            _sequenceHandle = default;
        }

        /// <summary>
        /// 戻りブレンド時間の取得
        /// </summary>
        protected override float GetOutBlendDurationInternal(ControllerActorAction action) {
            return action.outBlend;
        }
    }
}