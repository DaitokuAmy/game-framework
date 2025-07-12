using GameFramework.GimmickSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyStateGimmickSingleEvent : BodyGimmickSingleEvent {
        [Tooltip("変更するステート名")]
        public string stateName = "";
        [Tooltip("即時遷移するか")]
        public bool immediate = false;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyStateGimmickSingleEventHandler : BodyGimmickSingleEventHandler<StateGimmick, BodyStateGimmickSingleEvent> {
        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected override void OnInvokeInternal(BodyStateGimmickSingleEvent sequenceEvent, StateGimmick[] gimmicks) {
            gimmicks.Change(sequenceEvent.stateName, sequenceEvent.immediate);
        }
    }
}
