using GameFramework.BodySystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyAnimationGimmickRangeEvent : BodyGimmickRangeEvent {
        public enum PlayType {
            Play,
            Resume
        }

        [Tooltip("再生タイプ")]
        public PlayType playType = PlayType.Play;
        [Tooltip("反転再生か")]
        public bool reverse;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyAnimationGimmickRangeEventHandler : BodyGimmickRangeEventHandler<AnimationGimmick, BodyAnimationGimmickRangeEvent> {
        /// <summary>
        /// 入り処理
        /// </summary>
        protected override void OnEnterInternal(BodyAnimationGimmickRangeEvent sequenceEvent, AnimationGimmick[] gimmicks) {
            switch (sequenceEvent.playType) {
                case BodyAnimationGimmickRangeEvent.PlayType.Play:
                    gimmicks.Play(sequenceEvent.reverse);
                    break;
                case BodyAnimationGimmickRangeEvent.PlayType.Resume:
                    gimmicks.Resume(sequenceEvent.reverse);
                    break;
            }
        }
        
        /// <summary>
        /// 抜け処理
        /// </summary>
        protected override void OnExitInternal(BodyAnimationGimmickRangeEvent sequenceEvent, AnimationGimmick[] gimmicks) {
            switch (sequenceEvent.playType) {
                case BodyAnimationGimmickRangeEvent.PlayType.Play:
                    gimmicks.Play(!sequenceEvent.reverse);
                    break;
                case BodyAnimationGimmickRangeEvent.PlayType.Resume:
                    gimmicks.Resume(!sequenceEvent.reverse);
                    break;
            }
        }
    }
}
