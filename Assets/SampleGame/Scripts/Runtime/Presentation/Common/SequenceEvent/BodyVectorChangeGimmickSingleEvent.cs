using UnityEngine;

namespace SampleGame.Presentation {
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
