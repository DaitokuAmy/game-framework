using ActionSequencer;
using GameFramework.BodySystems;
using GameFramework.VfxSystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のエフェクト再生イベント
    /// </summary>
    public class BodyEffectSingleEvent : SignalSequenceEvent {
        [Tooltip("座標基準ロケーター名")]
        public string positionLocator = "";
        [Tooltip("角度基準ロケーター名")]
        public string rotationLocator = "";
        
        [Space]
        [Tooltip("再生情報")]
        public VfxManager.Context context;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class BodyEffectSingleEventHandler : SignalSequenceEventHandler<BodyEffectSingleEvent> {
        private VfxManager _vfxManager;
        private Body _body;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(VfxManager manager, Body body) {
            _vfxManager = manager;
            _body = body;
        }
        
        /// <summary>
        /// 実行時処理
        /// </summary>
        protected override void OnInvoke(BodyEffectSingleEvent sequenceEvent) {
            if (_vfxManager == null) {
                return;
            }

            _vfxManager.Play(
                sequenceEvent.context,
                GetLocator(sequenceEvent.positionLocator),
                GetLocator(sequenceEvent.rotationLocator),
                _body?.LayeredTime);
        }

        /// <summary>
        /// Locatorの取得
        /// </summary>
        private Transform GetLocator(string locatorName) {
            if (_body == null) {
                return null;
            }

            return _body.Locators[locatorName];
        }
    }
}
