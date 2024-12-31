using System.Collections;
using System.Collections.Generic;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Infrastructure;

namespace SampleGame.Presentation {
    /// <summary>
    /// ActorAction再生制御用クラス(時間待機)
    /// </summary>
    public class TimerActorActionResolver : ActorActionResolver<TimerActorAction> {
        private readonly SequenceController _sequenceController;

        // 再生中のSequenceHandle
        private readonly List<SequenceHandle> _sequenceHandles = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sequenceController">シーケンス再生用</param>
        public TimerActorActionResolver(SequenceController sequenceController) {
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        protected override void StartInternal(TimerActorAction action, object[] args) {
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayActionRoutineInternal(TimerActorAction action, object[] args) {
            // シーケンス再生
            foreach (var clip in action.sequenceClips) {
                _sequenceHandles.Add(_sequenceController.Play(clip));
            }
            
            // 時間待機
            yield return LayeredTime.WaitForSeconds(action.duration);

            _sequenceHandles.Clear();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelActionInternal(TimerActorAction action) {
            foreach (var handle in _sequenceHandles) {
                _sequenceController.Stop(handle);
            }

            _sequenceHandles.Clear();

            if (action.cancelSequenceClips.Length <= 0) {
                return;
            }

            foreach (var clip in action.cancelSequenceClips) {
                _sequenceController.Play(clip);
            }
        }
    }
}