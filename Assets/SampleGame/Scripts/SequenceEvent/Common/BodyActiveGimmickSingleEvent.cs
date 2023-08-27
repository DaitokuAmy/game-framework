using GameFramework.BodySystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyActiveGimmickSingleEvent : BodyGimmickSingleEvent {
        [Tooltip("変更するギミックのアクティブ状態")]
        public bool isActiveGimmick;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyActiveGimmickSingleEventHandler : BodyGimmickSingleEventHandler<ActiveGimmick, BodyActiveGimmickSingleEvent> {
        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected override void OnInvokeInternal(BodyActiveGimmickSingleEvent sequenceEvent, ActiveGimmick[] gimmicks) {
            if (sequenceEvent.isActiveGimmick) {
                gimmicks.Activate();
            }
            else {
                gimmicks.Deactivate();
            }
        }
    }
}
