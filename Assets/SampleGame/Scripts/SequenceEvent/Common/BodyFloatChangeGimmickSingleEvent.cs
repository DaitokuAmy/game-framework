namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyFloatChangeGimmickSingleEvent : BodyChangeGimmickSingleEvent<float> {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyFloatChangeGimmickSingleEventHandler : BodyChangeGimmickSingleEventHandler<float, BodyFloatChangeGimmickSingleEvent> {
    }
}
