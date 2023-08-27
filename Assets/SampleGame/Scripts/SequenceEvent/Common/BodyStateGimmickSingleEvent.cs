using GameFramework.BodySystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyStateGimmickSingleEvent : BodyGimmickSingleEvent {
        [Tooltip("変更するステート名")]
        public string stateName = "";
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyStateGimmickSingleEventHandler : BodyGimmickSingleEventHandler<StateGimmick, BodyStateGimmickSingleEvent> {
        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected override void OnInvokeInternal(BodyStateGimmickSingleEvent sequenceEvent, StateGimmick[] gimmicks) {
            gimmicks.Change(sequenceEvent.stateName);
        }
    }
}
