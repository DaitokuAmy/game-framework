namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用のドメインサービス
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    partial class BattleDomainService {
        /// <summary>
        /// 終了ステート
        /// </summary>
        private class FinishState : StateBase {
            /// <inheritdoc/>
            protected override BattleSequenceType SequenceType => BattleSequenceType.Finish;
            
            /// <inheritdoc/>
            public FinishState(BattleDomainService owner) : base(owner) {}
        }
    }
}