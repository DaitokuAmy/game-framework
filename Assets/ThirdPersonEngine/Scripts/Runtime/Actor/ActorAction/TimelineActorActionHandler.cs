using System.Collections;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using ThirdPersonEngine;
using UnityEngine.Playables;

namespace ThirdPersonEngine {
    /// <summary>
    /// ActorAction再生制御用クラス(TimelineAsset)
    /// </summary>
    public class TimelineActorActionHandler : ActorActionHandler<TimelineActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;

        private SequenceHandle _sequenceHandle;
        private PlayableBinding _binding;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">Motion再生用のHandle</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public TimelineActorActionHandler(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayRoutineInternal(TimelineActorAction action) {
            // シーケンス再生
            if (action.sequenceClip != null) {
                _sequenceHandle = _sequenceController.Play(action.sequenceClip);
            }

            // Timelineを再生する
            _motionHandle.Change(action.timelineAsset, action.inBlend);

            // クリップが流れるのを待つ
            var duration = (float)action.timelineAsset.duration - action.outBlend;
            yield return WaitForSeconds(duration);
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelInternal(TimelineActorAction action) {
            _sequenceHandle.Dispose();
            _sequenceHandle = default;
        }

        /// <summary>
        /// 戻りブレンド時間の取得
        /// </summary>
        protected override float GetOutBlendDurationInternal(TimelineActorAction action) {
            return action.outBlend;
        }
    }
}