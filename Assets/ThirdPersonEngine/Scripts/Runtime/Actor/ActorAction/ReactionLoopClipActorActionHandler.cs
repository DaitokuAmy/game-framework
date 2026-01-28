using System.Collections;
using System.Collections.Generic;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using ThirdPersonEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// ActorAction再生制御用クラス(ReactionLoopClip用)
    /// </summary>
    public class ReactionLoopClipActorActionHandler : ActorActionHandler<ReactionLoopClipActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;
        private readonly List<SequenceHandle> _sequenceHandles = new();

        private bool? _result;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">Motion再生用のHandle</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public ReactionLoopClipActorActionHandler(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }
        
        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayRoutineInternal(ReactionLoopClipActorAction action) {
            _result = null;
            
            // Clipの再生
            IEnumerator PlayClipRoutine(ReactionLoopClipActorAction.ClipInfo clipInfo, bool looping) {
                // クリップを再生する
                _motionHandle.Change(clipInfo.animationClip, clipInfo.inBlend);

                // シーケンス再生
                if (clipInfo.sequenceClip != null) {
                    _sequenceHandles.Add(_sequenceController.Play(clipInfo.sequenceClip));
                }

                // AnimationClipがなければ何もしない
                if (clipInfo.animationClip == null) {
                    yield break;
                }

                // Loop
                if (looping) {
                    // 進行されるのを待つ
                    while (_result == null) {
                        yield return null;
                    }
                }
                // OneShot
                else {
                    // クリップが流れるのを待つ
                    var duration = clipInfo.animationClip.length - clipInfo.outBlend;
                    yield return WaitForSeconds(duration);
                }
            }

            yield return PlayClipRoutine(action.inClipInfo, false);
            yield return PlayClipRoutine(action.loopClipInfo, true);
            if (!_result.HasValue || _result.Value) {
                yield return PlayClipRoutine(action.successOutClipInfo, false);
            }
            else {
                yield return PlayClipRoutine(action.failureOutClipInfo, false);
            }

            _sequenceHandles.Clear();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelInternal(ReactionLoopClipActorAction action) {
            foreach (var handle in _sequenceHandles) {
                handle.Dispose();
            }
            _sequenceHandles.Clear();

            if (action.cancelSequenceClip != null) {
                _sequenceController.Play(action.cancelSequenceClip);
            }
        }

        /// <summary>
        /// アクションの遷移
        /// </summary>
        protected override bool NextInternal(object[] args) {
            if (_result.HasValue) {
                return false;
            }

            _result = args.Length <= 0 || (bool)args[0];
            return true;
        }

        /// <summary>
        /// 戻りブレンド時間の取得
        /// </summary>
        protected override float GetOutBlendDurationInternal(ReactionLoopClipActorAction action) {
            var success = !_result.HasValue || _result.Value;
            return success ? action.successOutClipInfo.outBlend : action.failureOutClipInfo.outBlend;
        }
    }
}