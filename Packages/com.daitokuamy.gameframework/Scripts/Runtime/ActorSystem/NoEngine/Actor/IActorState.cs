using System;
using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクターステートマシン用のステートインターフェース
    /// </summary>
    public interface IActorState {
        /// <summary>
        /// 開始処理
        /// </summary>
        void Enter();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="commands">処理対象のコマンドリスト</param>
        /// <param name="signals">処理対象のシグナルリスト</param>
        /// <param name="deltaTime">変位時間</param>
        /// <returns>遷移先のState型(nullなら維持)</returns>
        Type Update(IReadOnlyList<ActorCommand> commands, IReadOnlyList<ActorSignal> signals, float deltaTime);

        /// <summary>
        /// 終了処理
        /// </summary>
        void Exit();
    }
}
