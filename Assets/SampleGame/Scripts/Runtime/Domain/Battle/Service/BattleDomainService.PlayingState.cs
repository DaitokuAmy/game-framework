namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用のドメインサービス
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    partial class BattleDomainService {
        /// <summary>
        /// プレイ中ステート
        /// </summary>
        private class PlayingState : StateBase {
            /// <inheritdoc/>
            protected override BattleSequenceType SequenceType => BattleSequenceType.Playing;
            
            /// <inheritdoc/>
            public PlayingState(BattleDomainService owner) : base(owner) {}
        }
    }
}