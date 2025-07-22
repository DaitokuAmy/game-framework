using ActionSequencer;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 重力係数制御用のイベント
    /// </summary>
    public class GravityScaleRangeEvent : RangeSequenceEvent {
        [Tooltip("変化させるスケール値")]
        public float scale;
    }

    /// <summary>
    /// イベントハンドラー
    /// </summary>
    public class GravityScaleRangeEventHandler : RangeSequenceEventHandler<GravityScaleRangeEvent> {
        private VelocityActorComponent _component;
        private ActorControlLayerType _layerType;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(VelocityActorComponent component, ActorControlLayerType layerType) {
            _component = component;
            _layerType = layerType;
        }
        
        /// <summary>
        /// 開始処理
        /// </summary>
        protected override void OnEnter(GravityScaleRangeEvent sequenceEvent) {
            _component.SetGravityScale(_layerType, sequenceEvent.scale);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        protected override void OnExit(GravityScaleRangeEvent sequenceEvent) {
            _component.ResetGravityScale(_layerType);
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void OnCancel(GravityScaleRangeEvent sequenceEvent) {
            OnExit(sequenceEvent);
        }
    }
}