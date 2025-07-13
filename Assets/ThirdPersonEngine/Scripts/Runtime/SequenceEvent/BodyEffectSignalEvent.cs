using ActionSequencer;
using GameFramework.ActorSystems;
using GameFramework.VfxSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// Body経由でのエフェクト再生用のイベント
    /// </summary>
    public class BodyEffectSignalEvent : SignalSequenceEvent {
        [Tooltip("エフェクトの再生情報")]
        public VfxContext context;

        [Header("ロケーター情報")]
        [Tooltip("座標基準となるLocator名")]
        public string positionLocator;
        [Tooltip("回転基準となるLocator名")]
        public string rotationLocator;
    }

    /// <summary>
    /// Body経由でのエフェクト再生用のイベントのハンドラ
    /// </summary>
    public class BodyEffectSignalEventHandler : SignalSequenceEventHandler<BodyEffectSignalEvent> {
        private VfxManager _vfxManager;
        private Body _owner;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(VfxManager vfxManager, Body owner) {
            _vfxManager = vfxManager;
            _owner = owner;
        }

        /// <summary>
        /// 通知時処理
        /// </summary>
        protected override void OnInvoke(BodyEffectSignalEvent sequenceEvent) {
            var positionRoot = _owner?.Locators[sequenceEvent.positionLocator];
            var rotationRoot = _owner?.Locators[sequenceEvent.rotationLocator];
            var context = sequenceEvent.context;
            if (_owner != null) {
                context.localScale *= _owner.BaseScale;
            }

            _vfxManager.Play(context, positionRoot, rotationRoot, _owner?.LayeredTime, null, _owner?.GameObject.layer ?? -1);
        }
    }
}