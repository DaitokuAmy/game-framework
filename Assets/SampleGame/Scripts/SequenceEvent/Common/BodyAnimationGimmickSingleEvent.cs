using GameFramework.GimmickSystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyAnimationGimmickSingleEvent : BodyGimmickSingleEvent {
        public enum PlayType {
            Play,
            Resume
        }

        [Tooltip("再生タイプ")]
        public PlayType playType = PlayType.Play;
        [Tooltip("反転再生か")]
        public bool reverse;
        [Tooltip("即時遷移か")]
        public bool immediate = false;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyAnimationGimmickSingleEventHandler : BodyGimmickSingleEventHandler<AnimationGimmick, BodyAnimationGimmickSingleEvent> {
        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected override void OnInvokeInternal(BodyAnimationGimmickSingleEvent sequenceEvent, AnimationGimmick[] gimmicks) {
            switch (sequenceEvent.playType) {
                case BodyAnimationGimmickSingleEvent.PlayType.Play:
                    gimmicks.Play(sequenceEvent.reverse, sequenceEvent.immediate);
                    break;
                case BodyAnimationGimmickSingleEvent.PlayType.Resume:
                    if (sequenceEvent.immediate) {
                        gimmicks.Play(sequenceEvent.reverse, sequenceEvent.immediate);
                    }
                    else {
                        gimmicks.Resume(sequenceEvent.reverse);
                    }
                    break;
            }
        }
    }
}
