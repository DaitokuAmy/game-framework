namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクターコントローラー(操作用)のインターフェース
    /// </summary>
    public interface IActorController : IActorInterface {
        /// <summary>
        /// Actorに管理された際の処理
        /// </summary>
        /// <param name="commandInputPort">コマンド追加用のPort</param>
        void Attached(IActorCommandInputPort commandInputPort);

        /// <summary>
        /// Actor管理から外された際の処理
        /// </summary>
        void Detached();
    }
}