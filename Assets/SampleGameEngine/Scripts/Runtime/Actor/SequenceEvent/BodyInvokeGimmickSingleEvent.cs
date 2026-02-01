using GameFramework.GimmickSystem;

namespace SampleGameEngine {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public sealed class BodyInvokeGimmickSingleEvent : BodyGimmickSingleEvent {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public sealed class BodyInvokeGimmickSingleEventHandler : BodyGimmickSingleEventHandler<InvokeGimmick, BodyInvokeGimmickSingleEvent> {
        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected override void OnInvokeInternal(BodyInvokeGimmickSingleEvent sequenceEvent, InvokeGimmick[] gimmicks) {
            gimmicks.Invoke();
        }
    }
}
