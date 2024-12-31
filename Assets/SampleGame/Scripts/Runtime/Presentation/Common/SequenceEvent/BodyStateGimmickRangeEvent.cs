using GameFramework.GimmickSystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyStateGimmickRangeEvent : BodyGimmickRangeEvent {
        [Tooltip("入った時に変更するステート名")]
        public string enterStateName = "";
        [Tooltip("入った時の変更を即時にするか")]
        public bool enterImmediate = false;
        [Tooltip("抜ける時に戻すステート名")]
        public string exitStateName = "";
        [Tooltip("抜けた時の変更を即時にするか")]
        public bool exitImmediate = false;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyStateGimmickRangeEventHandler : BodyGimmickRangeEventHandler<StateGimmick, BodyStateGimmickRangeEvent> {
        /// <summary>
        /// 入り処理
        /// </summary>
        protected override void OnEnterInternal(BodyStateGimmickRangeEvent sequenceEvent, StateGimmick[] gimmicks) {
            gimmicks.Change(sequenceEvent.enterStateName, sequenceEvent.enterImmediate);
        }
        
        /// <summary>
        /// 抜け処理
        /// </summary>
        protected override void OnExitInternal(BodyStateGimmickRangeEvent sequenceEvent, StateGimmick[] gimmicks) {
            gimmicks.Change(sequenceEvent.exitStateName, sequenceEvent.exitImmediate);
        }
    }
}
