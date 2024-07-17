using System.Collections;
using System.Collections.Generic;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.Core;
using GameFramework.PlayableSystems;
using UnityEngine;
using UnityEngine.Playables;

namespace SampleGame.Domain.Common {
    /// <summary>
    /// ActorAction再生制御用クラス(TimelineAsset)
    /// </summary>
    public class TimelineActorActionResolver : ActorActionResolver<TimelineActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;

        // 再生中のSequenceHandle
        private readonly List<SequenceHandle> _sequenceHandles = new();

        private PlayableBinding _binding;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">Motion再生用のHandle</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public TimelineActorActionResolver(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayActionRoutineInternal(TimelineActorAction action, object[] args) {
            // シーケンス再生
            foreach (var clip in action.sequenceClips) {
                _sequenceHandles.Add(_sequenceController.Play(clip));
            }
            
            // Timelineを再生する
            _motionHandle.Change(action.timelineAsset, action.inBlend);
  
            // クリップが流れるのを待つ
            var duration = (float)action.timelineAsset.duration - action.outBlend;
            if (LayeredTime != null) {
                yield return LayeredTime.WaitForSeconds(duration);
            }
            else {
                yield return new WaitForSeconds(duration);
            }

            _sequenceHandles.Clear();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelActionInternal(TimelineActorAction action) {
            foreach (var handle in _sequenceHandles) {
                _sequenceController.Stop(handle);
            }

            _sequenceHandles.Clear();
        }

        /// <summary>
        /// 戻りブレンド時間の取得
        /// </summary>
        protected override float GetOutBlendDurationInternal(TimelineActorAction action) {
            return action.outBlend;
        }
    }
}