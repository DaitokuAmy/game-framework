using System.Collections;
using ActionSequencer;
using GameFramework.ActorSystems;
using SampleGame.Infrastructure;

namespace SampleGame.Presentation {
    /// <summary>
    /// ActorAction再生制御用クラス(時間待機)
    /// </summary>
    public class TimerActorActionHandler : ActorActionHandler<TimerActorAction> {
        private readonly SequenceController _sequenceController;

        private SequenceHandle _sequenceHandle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sequenceController">シーケンス再生用</param>
        public TimerActorActionHandler(SequenceController sequenceController) {
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayRoutineInternal(TimerActorAction action) {
            // シーケンス再生
            if (action.sequenceClip != null) {
                _sequenceHandle = _sequenceController.Play(action.sequenceClip);
            }
            
            // 時間待機
            yield return WaitForSeconds(action.duration);
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelInternal(TimerActorAction action) {
            _sequenceHandle.Dispose();
            _sequenceHandle = default;

            if (action.cancelSequenceClip != null) {
                _sequenceController.Play(action.cancelSequenceClip);
            }
        }
    }
}