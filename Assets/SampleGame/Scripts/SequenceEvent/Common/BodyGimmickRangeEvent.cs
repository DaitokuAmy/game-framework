using ActionSequencer;
using GameFramework.BodySystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public abstract class BodyGimmickRangeEvent : RangeSequenceEvent {
        [Tooltip("キャンセル時に終了処理を呼ぶか")]
        public bool exitOnCanceled = true;
        [Tooltip("ギミック名")]
        public string gimmickName;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public abstract class BodyGimmickRangeEventHandler<TGimmick, TEvent> : RangeSequenceEventHandler<TEvent>
        where TGimmick : Gimmick
        where TEvent : BodyGimmickRangeEvent {
        private GimmickController _gimmickController;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(GimmickController gimmickController) {
            _gimmickController = gimmickController;
        }

        /// <summary>
        /// 入り処理
        /// </summary>
        protected sealed override void OnEnter(TEvent sequenceEvent) {
            if (_gimmickController == null) {
                return;
            }

            var gimmicks = _gimmickController.GetGimmicks<TGimmick>(sequenceEvent.gimmickName);
            OnEnterInternal(sequenceEvent, gimmicks);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected sealed override void OnUpdate(TEvent sequenceEvent, float elapsedTime) {
            var gimmicks = _gimmickController.GetGimmicks<TGimmick>(sequenceEvent.gimmickName);
            OnUpdateInternal(sequenceEvent, elapsedTime, gimmicks);
        }

        /// <summary>
        /// 抜け処理
        /// </summary>
        protected sealed override void OnExit(TEvent sequenceEvent) {
            var gimmicks = _gimmickController.GetGimmicks<TGimmick>(sequenceEvent.gimmickName);
            OnExitInternal(sequenceEvent, gimmicks);
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected sealed override void OnCancel(TEvent sequenceEvent) {
            if (sequenceEvent.exitOnCanceled) {
                OnExit(sequenceEvent);
            }
        }

        /// <summary>
        /// 入り処理
        /// </summary>
        protected virtual void OnEnterInternal(TEvent sequenceEvent, TGimmick[] gimmicks) {}

        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void OnUpdateInternal(TEvent sequenceEvent, float elapsedTime, TGimmick[] gimmicks) {}

        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void OnExitInternal(TEvent sequenceEvent, TGimmick[] gimmicks) {}
    }
}
