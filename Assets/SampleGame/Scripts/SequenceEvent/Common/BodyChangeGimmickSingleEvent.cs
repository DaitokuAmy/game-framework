using GameFramework.BodySystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
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
    public abstract class BodyChangeGimmickSingleEventHandler<T> : BodyGimmickSingleEventHandler<ChangeGimmick<T>, BodyChangeGimmickSingleEvent<T>> {
        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected override void OnInvokeInternal(BodyChangeGimmickSingleEvent<T> sequenceEvent, ChangeGimmick<T>[] gimmicks) {
            gimmicks.Change(sequenceEvent.target, sequenceEvent.duration);
        }
    }
}
