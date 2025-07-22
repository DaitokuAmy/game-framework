using GameFramework;
using GameFramework.Core;

namespace ThirdPersonEngine {
    /// <summary>
    /// バトルキャラアクター - SequenceEvent初期化
    /// </summary>
    partial class BattleCharacterActor {
        /// <summary>
        /// シーケンスイベントの初期化
        /// </summary>
        private void SetupSequenceEvents(IScope scope) {
            SequenceControllerInternal.BindRangeEventHandler<GravityScaleRangeEvent, GravityScaleRangeEventHandler>(handler => { handler.Setup(_velocityComponent, ActorControlLayerType.Self); })
                .RegisterTo(scope);
        }
    }
}