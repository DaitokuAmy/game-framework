using System;
using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクターステートマシン用インターフェース
    /// </summary>
    public interface IActorStateMachine : IDisposable {
        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="commands">処理対象のコマンドリスト</param>
        /// <param name="signals">処理対象のシグナルリスト</param>
        /// <param name="deltaTime">変位時間</param>
        void Update(IReadOnlyList<ActorCommand> commands, IReadOnlyList<ActorSignal> signals, float deltaTime);
    }
}
