using System;
using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクターステートマシン用のステート基底
    /// </summary>
    public abstract class ActorState<TKey, TBlackboard> : IActorState
        where TBlackboard : class, IActorStateBlackboard, new() {
        private ActorStateMachine<TKey, TBlackboard> _stateMachine;

        /// <summary>所有主アクター</summary>
        protected Actor<TKey> Owner => _stateMachine?.Owner;
        /// <summary>ブラックボード</summary>
        protected TBlackboard Blackboard => _stateMachine?.Blackboard;

        /// <inheritdoc/>
        void IActorState.Enter() => Enter();

        /// <inheritdoc/>
        Type IActorState.Update(IReadOnlyList<ActorCommand> commands, IReadOnlyList<ActorSignal> signals, float deltaTime) => Update(commands, signals, deltaTime);

        /// <inheritdoc/>
        void IActorState.Exit() => Exit();

        /// <summary>
        /// セットアップ処理
        /// </summary>
        /// <param name="stateMachine">所有主ステートマシン</param>
        internal void Setup(ActorStateMachine<TKey, TBlackboard> stateMachine) {
            _stateMachine = stateMachine;
            Setup();
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        protected virtual void Setup() { }

        /// <summary>
        /// 開始処理
        /// </summary>
        protected virtual void Enter() { }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="commands">処理対象のコマンドリスト</param>
        /// <param name="signals">処理対象のシグナルリスト</param>
        /// <param name="deltaTime">変位時間</param>
        /// <returns>遷移先のState型(nullなら維持)</returns>
        protected virtual Type Update(IReadOnlyList<ActorCommand> commands, IReadOnlyList<ActorSignal> signals, float deltaTime) { return null; }

        /// <summary>
        /// 終了処理
        /// </summary>
        protected virtual void Exit() { }
    }
}