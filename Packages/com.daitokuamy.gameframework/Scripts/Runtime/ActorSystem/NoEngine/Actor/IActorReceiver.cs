namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクターイベント連携用のインターフェース
    /// </summary>
    public interface IActorReceiver : IActorInterface {
        /// <summary>
        /// Actorに管理された際の処理
        /// </summary>
        /// <param name="signalInputPort">シグナル追加用のPort</param>
        void Attached(IActorSignalInputPort signalInputPort);

        /// <summary>
        /// Actor管理から外された際の処理
        /// </summary>
        void Detached();
    }
}