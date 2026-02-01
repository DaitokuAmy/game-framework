using GameFramework.GimmickSystem;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public sealed class BodyActiveGimmickSingleEvent : BodyGimmickSingleEvent {
        [Tooltip("変更するギミックのアクティブ状態")]
        public bool isActiveGimmick;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public sealed class BodyActiveGimmickSingleEventHandler : BodyGimmickSingleEventHandler<ActiveGimmick, BodyActiveGimmickSingleEvent> {
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
