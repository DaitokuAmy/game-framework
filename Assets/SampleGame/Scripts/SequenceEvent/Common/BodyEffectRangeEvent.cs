using ActionSequencer;
using GameFramework.BodySystems;
using GameFramework.VfxSystems;
using UnityEngine;

namespace SampleGame.SequenceEvents {
    /// <summary>
    /// Body用のエフェクト再生イベント(Loop用)
    /// </summary>
    public class BodyEffectRangeEvent : RangeSequenceEvent {
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
    public class BodyEffectRangeEventHandler : RangeSequenceEventHandler<BodyEffectRangeEvent> {
        private VfxManager _vfxManager;
        private Body _body;
        private VfxManager.Handle _handle;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(VfxManager manager, Body body) {
            _vfxManager = manager;
            _body = body;
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void OnEnter(BodyEffectRangeEvent sequenceEvent) {
            if (_vfxManager == null) {
                return;
            }

            _handle = _vfxManager.Play(
                sequenceEvent.context,
                GetLocator(sequenceEvent.positionLocator),
                GetLocator(sequenceEvent.rotationLocator),
                _body?.LayeredTime);
        }

        /// <summary>
        /// 終了時処理
        /// </summary>
        protected override void OnExit(BodyEffectRangeEvent sequenceEvent) {
            _handle.Stop();
        }

        /// <summary>
        /// キャンセル時処理
        /// </summary>
        protected override void OnCancel(BodyEffectRangeEvent sequenceEvent) {
            OnExit(sequenceEvent);
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
