namespace GameFramework.ActorSystem {
    /// <summary>
    /// Actorにシグナルを提供するためのインターフェース
    /// </summary>
    public interface IActorSignalInputPort {
        /// <summary>
        /// シグナルの生成
        /// </summary>
        T CreateSignal<T>() where T : ActorSignal, new();

        /// <summary>
        /// シグナルの追加
        /// </summary>
        /// <param name="signal">追加対象のシグナル</param>
        void AddSignal(ActorSignal signal);
    }
}
