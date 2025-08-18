using GameFramework;
using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// バトル用のドメインサービス
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    partial class BattleDomainService {
        /// <summary>
        /// BattleSequenceにおけるState基底
        /// </summary>
        private abstract class StateBase : IState<BattleSequenceType> {
            protected BattleDomainService Owner { get; }
            
            /// <inheritdoc/>
            BattleSequenceType IState<BattleSequenceType>.Key => SequenceType;

            /// <summary>自身が表すStateのSequenceType</summary>
            protected abstract BattleSequenceType SequenceType { get; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            protected StateBase(BattleDomainService owner) {
                Owner = owner;
            }

            /// <inheritdoc/>
            void IState<BattleSequenceType>.OnEnter(BattleSequenceType prevKey, IScope scope) {
                DebugLog.Info($"Enter BattleSequence:{GetType().Name}");
                OnEnterInternal(scope);
            }

            /// <inheritdoc/>
            void IState<BattleSequenceType>.OnUpdate(float deltaTime) {
                OnUpdateInternal(deltaTime);
            }

            /// <inheritdoc/>
            void IState<BattleSequenceType>.OnExit(BattleSequenceType nextKey) {
                OnExitInternal();
                DebugLog.Info($"Exit BattleSequence:{GetType().Name}");
            }

            /// <summary>
            /// 開始時処理
            /// </summary>
            protected virtual void OnEnterInternal(IScope scope) { }

            /// <summary>
            /// 更新時処理
            /// </summary>
            protected virtual void OnUpdateInternal(float deltaTime) { }

            /// <summary>
            /// 終了時処理
            /// </summary>
            protected virtual void OnExitInternal() { }

            /// <summary>
            /// ステートの変更
            /// </summary>
            protected void ChangeState(BattleSequenceType nextKey) {
                Owner.BattleModelInternal.ChangeState(nextKey);
            }
        }
    }
}