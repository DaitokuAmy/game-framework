using GameFramework.GimmickSystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public abstract class BodyChangeGimmickSingleEvent<T> : BodyGimmickSingleEvent {
        [Tooltip("変更先の値")]
        public T target = default;
        [Tooltip("変更にかける時間")]
        public float duration = 0.0f;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public abstract class BodyChangeGimmickSingleEventHandler<T, TEvent> : BodyGimmickSingleEventHandler<ChangeGimmick<T>, TEvent>
        where TEvent : BodyChangeGimmickSingleEvent<T> {
        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected override void OnInvokeInternal(TEvent sequenceEvent, ChangeGimmick<T>[] gimmicks) {
            gimmicks.Change(sequenceEvent.target, sequenceEvent.duration);
        }
    }
}
