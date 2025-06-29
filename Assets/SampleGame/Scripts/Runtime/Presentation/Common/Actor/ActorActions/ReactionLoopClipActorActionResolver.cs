using System.Collections;
using System.Collections.Generic;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using GameFramework;
using SampleGame.Infrastructure;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// ActorAction再生制御用クラス(ReactionLoopClip用)
    /// </summary>
    public class ReactionLoopClipActorActionResolver : ActorActionResolver<ReactionLoopClipActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;

        // 再生中のSequenceHandle
        private readonly List<SequenceHandle> _sequenceHandles = new();

        // ループ抜けの判定結果
        private bool? _result;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">Motion再生用のHandle</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public ReactionLoopClipActorActionResolver(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        protected override void StartInternal(ReactionLoopClipActorAction action, object[] args) {
            _result = null;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayActionRoutineInternal(ReactionLoopClipActorAction action, object[] args) {
            // Clipの再生
            IEnumerator PlayClipRoutine(ReactionLoopClipActorAction.ClipInfo clipInfo, bool looping) {
                // クリップを再生する
                _motionHandle.Change(clipInfo.animationClip, clipInfo.inBlend);

                // シーケンス再生
                foreach (var clip in clipInfo.sequenceClips) {
                    _sequenceHandles.Add(_sequenceController.Play(clip));
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
                    if (LayeredTime != null) {
                        yield return LayeredTime.WaitForSeconds(duration);
                    }
                    else {
                        yield return new WaitForSeconds(duration);
                    }
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
        protected override void CancelActionInternal(ReactionLoopClipActorAction action) {
            foreach (var handle in _sequenceHandles) {
                _sequenceController.Stop(handle);
            }

            _sequenceHandles.Clear();

            if (action.cancelSequenceClips == null) {
                return;
            }

            foreach (var clip in action.cancelSequenceClips) {
                _sequenceController.Play(clip);
            }
        }

        /// <summary>
        /// アクションの遷移
        /// </summary>
        protected override bool NextActionInternal(object[] args) {
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