using System.Collections;
using System.Collections.Generic;
using ActionSequencer;
using GameFramework.BodySystems;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// ActorAction再生制御用クラス(AnimationClip)
    /// </summary>
    public class ClipActorActionResolver : ActorActionResolver<ClipActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;

        // 再生中のSequenceHandle
        private readonly List<SequenceHandle> _sequenceHandles = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">Motion再生用のHandle</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public ClipActorActionResolver(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayActionRoutineInternal(ClipActorAction action, object[] args) {
            // クリップを再生する
            _motionHandle.Change(action.animationClip, action.inBlend);

            // シーケンス再生
            foreach (var clip in action.sequenceClips) {
                _sequenceHandles.Add(_sequenceController.Play(clip));
            }

            // クリップが流れるのを待つ
            var duration = action.animationClip.length - action.outBlend;
            if (LayeredTime != null) {
                yield return LayeredTime.WaitForSeconds(duration);
            }
            else {
                yield return new WaitForSeconds(action.animationClip.length);
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