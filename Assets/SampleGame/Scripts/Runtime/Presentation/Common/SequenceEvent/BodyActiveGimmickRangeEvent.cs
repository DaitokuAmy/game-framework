using GameFramework.GimmickSystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyActiveGimmickRangeEvent : BodyGimmickRangeEvent {
        public enum ActiveType {
            ActiveToInactive,
            InactiveToActive
        }

        [Tooltip("アクティブの切り替えタイプ")]
        public ActiveType activeType = ActiveType.ActiveToInactive;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyActiveGimmickRangeEventHandler : BodyGimmickRangeEventHandler<ActiveGimmick, BodyActiveGimmickRangeEvent> {
        /// <summary>
        /// 入り処理
        /// </summary>
        protected override void OnEnterInternal(BodyActiveGimmickRangeEvent sequenceEvent, ActiveGimmick[] gimmicks) {
            if (sequenceEvent.activeType == BodyActiveGimmickRangeEvent.ActiveType.ActiveToInactive) {
                gimmicks.Activate();
            }
            else {
                gimmicks.Deactivate();
            }
        }
        
        /// <summary>
        /// 抜け処理
        /// </summary>
        protected override void OnExitInternal(BodyActiveGimmickRangeEvent sequenceEvent, ActiveGimmick[] gimmicks) {
            if (sequenceEvent.activeType == BodyActiveGimmickRangeEvent.ActiveType.ActiveToInactive) {
                gimmicks.Deactivate();
            }
            else {
                gimmicks.Activate();
            }
        }
    }
}
