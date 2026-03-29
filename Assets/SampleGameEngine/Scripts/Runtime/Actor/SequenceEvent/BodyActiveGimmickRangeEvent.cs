using GameFramework.GimmickSystem;
using System.Collections.Generic;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public sealed class BodyActiveGimmickRangeEvent : BodyGimmickRangeEvent {
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
    public sealed class BodyActiveGimmickRangeEventHandler : BodyGimmickRangeEventHandler<ActiveGimmick, BodyActiveGimmickRangeEvent> {
        /// <summary>
        /// 入り処理
        /// </summary>
        protected override void OnEnterInternal(BodyActiveGimmickRangeEvent sequenceEvent, IReadOnlyList<ActiveGimmick> gimmicks) {
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
        protected override void OnExitInternal(BodyActiveGimmickRangeEvent sequenceEvent, IReadOnlyList<ActiveGimmick> gimmicks) {
            if (sequenceEvent.activeType == BodyActiveGimmickRangeEvent.ActiveType.ActiveToInactive) {
                gimmicks.Deactivate();
            }
            else {
                gimmicks.Activate();
            }
        }
    }
}
