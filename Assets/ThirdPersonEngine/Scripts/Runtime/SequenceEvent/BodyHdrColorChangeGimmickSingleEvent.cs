using GameFramework;

namespace ThirdPersonEngine {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyHdrColorChangeGimmickSingleEvent : BodyChangeGimmickSingleEvent<HdrColor> {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyHdrColorChangeGimmickSingleEventHandler : BodyChangeGimmickSingleEventHandler<HdrColor, BodyHdrColorChangeGimmickSingleEvent> {
    }
}
