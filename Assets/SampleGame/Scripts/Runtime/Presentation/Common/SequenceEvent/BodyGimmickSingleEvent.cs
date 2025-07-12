using ActionSequencer;
using GameFramework.ActorSystems;
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
        private GimmickComponent _gimmickComponent;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(GimmickComponent gimmickComponent) {
            _gimmickComponent = gimmickComponent;
        }
        
        /// <summary>
        /// 実行時処理
        /// </summary>
        protected sealed override void OnInvoke(TEvent sequenceEvent) {
            if (_gimmickComponent == null) {
                return;
            }

            var gimmicks = _gimmickComponent.GetGimmicks<TGimmick>(sequenceEvent.gimmickName);
            OnInvokeInternal(sequenceEvent, gimmicks);
        }

        /// <summary>
        /// ギミック実行時処理
        /// </summary>
        protected abstract void OnInvokeInternal(TEvent sequenceEvent, TGimmick[] gimmicks);
    }
}
