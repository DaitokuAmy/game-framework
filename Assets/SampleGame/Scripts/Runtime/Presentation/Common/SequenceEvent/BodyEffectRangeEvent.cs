using ActionSequencer;
using GameFramework.BodySystems;
using GameFramework.VfxSystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Body経由でのループエフェクト再生用のイベント
    /// </summary>
    public class BodyEffectRangeEvent : RangeSequenceEvent {
        [Tooltip("エフェクトの再生情報")]
        public VfxManager.Context context;

        [Header("ロケーター情報")]
        [Tooltip("座標基準となるLocator名")]
        public string positionLocator;
        [Tooltip("回転基準となるLocator名")]
        public string rotationLocator;
    }

    /// <summary>
    /// Body経由でのエフェクト再生用のイベントのハンドラ
    /// </summary>
    public class BodyEffectRangeEventHandler : RangeSequenceEventHandler<BodyEffectRangeEvent> {
        private VfxManager _vfxManager;
        private Body _owner;
        private VfxManager.Handle _handle;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(VfxManager vfxManager, Body owner) {
            _vfxManager = vfxManager;
            _owner = owner;
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void OnEnter(BodyEffectRangeEvent sequenceEvent) {
            var positionRoot = _owner?.Locators[sequenceEvent.positionLocator];
            var rotationRoot = _owner?.Locators[sequenceEvent.rotationLocator];
            var context = sequenceEvent.context;
            if (_owner != null) {
                context.localScale *= _owner.BaseScale;
            }
            _handle = _vfxManager.Play(context, positionRoot, rotationRoot, _owner?.LayeredTime, null, _owner?.GameObject.layer ?? -1);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        protected override void OnExit(BodyEffectRangeEvent sequenceEvent) {
            _handle.Stop();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void OnCancel(BodyEffectRangeEvent sequenceEvent) => OnExit(sequenceEvent);
    }
}