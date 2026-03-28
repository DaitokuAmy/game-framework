namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター更新管理インターフェース
    /// </summary>
    public interface IActorScheduler<TKey> {
        /// <summary>
        /// アクター管理用クラスの登録
        /// </summary>
        void RegisterManager(ActorManager<TKey> manager);

        /// <summary>
        /// アクター管理用クラスの登録解除
        /// </summary>
        void UnregisterManager(ActorManager<TKey> manager);
    }
}
