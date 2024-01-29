using GameFramework.GimmickSystems;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyInvokeGimmickSingleEvent : BodyGimmickSingleEvent {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyInvokeGimmickSingleEventHandler : BodyGimmickSingleEventHandler<InvokeGimmick, BodyInvokeGimmickSingleEvent> {
        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected override void OnInvokeInternal(BodyInvokeGimmickSingleEvent sequenceEvent, InvokeGimmick[] gimmicks) {
            gimmicks.Invoke();
        }
    }
}
