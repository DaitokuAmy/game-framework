using GameFramework.GimmickSystems;
using UnityEngine;

namespace SampleGame.Presentation {
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
        [Tooltip("入りが即時反映か")]
        public bool enterImmediate = false;
        [Tooltip("抜けが即時反映か")]
        public bool exitImmediate = false;
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
                    gimmicks.Play(sequenceEvent.reverse, sequenceEvent.enterImmediate);
                    break;
                case BodyAnimationGimmickRangeEvent.PlayType.Resume:
                    if (sequenceEvent.enterImmediate) {
                        gimmicks.Play(sequenceEvent.reverse, sequenceEvent.enterImmediate);
                    }
                    else {
                        gimmicks.Resume(sequenceEvent.reverse);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 抜け処理
        /// </summary>
        protected override void OnExitInternal(BodyAnimationGimmickRangeEvent sequenceEvent, AnimationGimmick[] gimmicks) {
            switch (sequenceEvent.playType) {
                case BodyAnimationGimmickRangeEvent.PlayType.Play:
                    gimmicks.Play(!sequenceEvent.reverse, sequenceEvent.exitImmediate);
                    break;
                case BodyAnimationGimmickRangeEvent.PlayType.Resume:
                    if (sequenceEvent.exitImmediate) {
                        gimmicks.Play(!sequenceEvent.reverse, sequenceEvent.exitImmediate);
                    }
                    else {
                        gimmicks.Resume(!sequenceEvent.reverse);
                    }
                    break;
            }
        }
    }
}
