using GameFramework;
using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用のドメインサービス
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    partial class BattleDomainService {
        /// <summary>
        /// 入り演出ステート
        /// </summary>
        private class EnterState : StateBase {
            /// <inheritdoc/>
            protected override BattleSequenceType SequenceType => BattleSequenceType.Enter;
            
            /// <inheritdoc/>
            public EnterState(BattleDomainService owner) : base(owner) {}

            /// <inheritdoc/>
            protected override void OnEnterInternal(IScope scope) {
                ChangeState(BattleSequenceType.Playing);
            }
        }
    }
}