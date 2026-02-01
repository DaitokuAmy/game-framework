using GameFramework;

namespace SampleGameEngine {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public sealed class BodyHdrColorChangeGimmickSingleEvent : BodyChangeGimmickSingleEvent<HdrColor> {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public sealed class BodyHdrColorChangeGimmickSingleEventHandler : BodyChangeGimmickSingleEventHandler<HdrColor, BodyHdrColorChangeGimmickSingleEvent> {
    }
}
