using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.PlayableSystems;
using GameFramework;
using ThirdPersonEngine;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// ActorAction再生制御用クラス(連続AnimationClip)
    /// </summary>
    public class SequentialClipActorActionHandler : ActorActionHandler<SequentialClipActorAction> {
        private readonly MotionHandle _motionHandle;
        private readonly SequenceController _sequenceController;
        private readonly List<SequenceHandle> _sequenceHandles = new();

        // 現在再生中のClipIndex
        private int _currentIndex;
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
        public SequentialClipActorActionHandler(MotionHandle motionHandle, SequenceController sequenceController) {
            _motionHandle = motionHandle;
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// アクションの再生コルーチン
        /// </summary>
        protected override IEnumerator PlayRoutineInternal(SequentialClipActorAction action) {
            _currentIndex = 0;
            _loopIndex = -1;
            _nextLoopIndex = -1;

            // LoopClipの総数を取得
            _loopCount = action.clipInfos.Count(x => x.animationClip != null && x.animationClip.isLooping);
            
            // Clipの再生
            IEnumerator PlayClipRoutine(SequentialClipActorAction.ClipInfo clipInfo) {
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
                if (clipInfo.animationClip.isLooping) {
                    var timer = clipInfo.loopDuration;

                    // 現在のLoopIndexを更新
                    _loopIndex++;
                    // 進行されるのを待つ
                    while (_nextLoopIndex < _loopIndex) {
                        // タイマー更新＆監視
                        timer -= DeltaTime;
                        if (clipInfo.loopDuration >= 0.0f) {
                            if (timer <= 0.0f) {
                                break;
                            }
                        }

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

            foreach (var info in action.clipInfos) {
                yield return PlayClipRoutine(info);
                _currentIndex++;
            }

            _sequenceHandles.Clear();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void CancelInternal(SequentialClipActorAction action) {
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
            if (action.clipInfos.Length <= 0) {
                return 0.0f;
            }

            return action.clipInfos[action.clipInfos.Length - 1].outBlend;
        }
    }
}