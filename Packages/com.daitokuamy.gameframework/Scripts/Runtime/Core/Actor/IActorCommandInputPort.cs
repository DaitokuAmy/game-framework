namespace GameFramework.Core {
    /// <summary>
    /// Actorにコマンドを提供するためのインターフェース
    /// </summary>
    public interface IActorCommandInputPort {
        /// <summary>
        /// コマンドの生成
        /// </summary>
        T CreateCommand<T>() where T : ActorCommand, new();

        /// <summary>
        /// コマンドの追加
        /// </summary>
        /// <param name="command">追加対象のコマンド</param>
        void AddCommand(ActorCommand command);
    }
}