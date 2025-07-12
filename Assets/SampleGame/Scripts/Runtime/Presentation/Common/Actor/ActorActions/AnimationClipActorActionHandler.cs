using System.Collections;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using SampleGame.Infrastructure;

namespace SampleGame.Presentation {
    /// <summary>
    /// ActorAction再生制御用クラス(AnimationClip)
    /// </summary>
    public class AnimationClipActorActionHandler : ActorActionHandler<AnimationClipActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;
        
        private SequenceHandle _sequenceHandle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">Motion再生用のHandle</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public AnimationClipActorActionHandler(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayRoutineInternal(AnimationClipActorAction action) {
            // クリップを再生する
            _motionHandle.Change(action.animationClip, action.inBlend);

            // シーケンス再生
            if (action.sequenceClip != null) {
                _sequenceHandle = _sequenceController.Play(action.sequenceClip);
            }

            // クリップが流れるのを待つ
            var duration = action.animationClip.length - action.outBlend;
            yield return WaitForSeconds(duration);
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelInternal(AnimationClipActorAction action) {
            _sequenceHandle.Dispose();
            _sequenceHandle = default;
        }

        /// <summary>
        /// 戻りブレンド時間の取得
        /// </summary>
        protected override float GetOutBlendDurationInternal(AnimationClipActorAction action) {
            return action.outBlend;
        }
    }
}