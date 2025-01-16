using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public class BodyColorChangeGimmickSingleEvent : BodyChangeGimmickSingleEvent<Color> {
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyColorChangeGimmickSingleEventHandler : BodyChangeGimmickSingleEventHandler<Color, BodyColorChangeGimmickSingleEvent> {
    }
}
