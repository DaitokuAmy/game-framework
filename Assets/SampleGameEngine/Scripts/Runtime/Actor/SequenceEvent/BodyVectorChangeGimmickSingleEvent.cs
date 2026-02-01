using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public sealed class BodyVectorChangeGimmickSingleEvent : BodyChangeGimmickSingleEvent<Vector4> {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public sealed class BodyVectorChangeGimmickSingleEventHandler : BodyChangeGimmickSingleEventHandler<Vector4, BodyVectorChangeGimmickSingleEvent> {
    }
}
