using ActionSequencer;
using GameFramework.BodySystems;
using GameFramework.GimmickSystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Body用のギミック制御イベント
    /// </summary>
    public abstract class BodyGimmickSingleEvent : SignalSequenceEvent {
        [Tooltip("ギミック名")]
        public string gimmickName;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public abstract class BodyGimmickSingleEventHandler<TGimmick, TEvent> : SignalSequenceEventHandler<TEvent>
        where TGimmick : Gimmick
        where TEvent : BodyGimmickSingleEvent {
        private GimmickController _gimmickController;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(GimmickController gimmickController) {
            _gimmickController = gimmickController;
        }
        
        /// <summary>
        /// 実行時処理
        /// </summary>
        protected sealed override void OnInvoke(TEvent sequenceEvent) {
            if (_gimmickController == null) {
                return;
            }

            var gimmicks = _gimmickController.GetGimmicks<TGimmick>(sequenceEvent.gimmickName);
            OnInvokeInternal(sequenceEvent, gimmicks);
        }

        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected abstract void OnInvokeInternal(TEvent sequenceEvent, TGimmick[] gimmicks);
    }
}
