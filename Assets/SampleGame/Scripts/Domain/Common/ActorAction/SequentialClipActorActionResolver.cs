using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame.Domain.Common {
    /// <summary>
    /// ActorAction再生制御用クラス(連続AnimationClip)
    /// </summary>
    public class SequentialClipActorActionResolver : ActorActionResolver<SequentialClipActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;

        // 再生中のSequenceHandle
        private readonly List<SequenceHandle> _sequenceHandles = new();
        
        // 現在のLoopIndex
        private int _loopIndex;
        // 進行されているLoopIndex
        private int _nextLoopIndex;
        // LoopClipの総数
        private int _loopCount;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="motionHandle">Motion再生用のHandle</param>
        /// <param name="sequenceController">シーケンス再生用</param>
        public SequentialClipActorActionResolver(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayActionRoutineInternal(SequentialClipActorAction action, object[] args) {
            // Clipの再生
            IEnumerator PlayClipRoutine(SequentialClipActorAction.ClipInfo clipInfo) {
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
                if (clipInfo.animationClip.isLooping) {
                    // 現在のLoopIndexを更新
                    _loopIndex++;
                    // 進行されるのを待つ
                    while (_nextLoopIndex < _loopIndex) {
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

            _loopIndex = -1;
            _nextLoopIndex = -1;

            // LoopClipの総数を取得
            _loopCount = action.clipInfos.Count(x => x.animationClip != null && x.animationClip.isLooping);

            foreach (var info in action.clipInfos) {
                yield return PlayClipRoutine(info);
            }

            _sequenceHandles.Clear();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelActionInternal(SequentialClipActorAction action) {
            foreach (var handle in _sequenceHandles) {
                _sequenceController.Stop(handle);
            }

            _sequenceHandles.Clear();

            foreach (var clip in action.cancelSequenceClips) {
                _sequenceController.Play(clip);
            }
        }

        /// <summary>
        /// アクションの遷移
        /// </summary>
        protected override bool NextActionInternal(object[] args) {
            if (_nextLoopIndex + 1 >= _loopCount) {
                return false;
            }
            
            _nextLoopIndex++;
            return true;
        }

        /// <summary>
        /// 戻りブレンド時間の取得
        /// </summary>
        protected override float GetOutBlendDurationInternal(SequentialClipActorAction action) {
            return action.clipInfos.Length > 0 ? action.clipInfos[action.clipInfos.Length - 1].outBlend : 0.0f;
        }
    }
}