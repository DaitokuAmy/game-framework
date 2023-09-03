using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyVectorChangeGimmickSingleEvent : BodyChangeGimmickSingleEvent<Vector4> {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyVectorChangeGimmickSingleEventHandler : BodyChangeGimmickSingleEventHandler<Vector4, BodyVectorChangeGimmickSingleEvent> {
    }
}
