using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public sealed class BodyColorChangeGimmickSingleEvent : BodyChangeGimmickSingleEvent<Color> {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public sealed class BodyColorChangeGimmickSingleEventHandler : BodyChangeGimmickSingleEventHandler<Color, BodyColorChangeGimmickSingleEvent> {
    }
}
