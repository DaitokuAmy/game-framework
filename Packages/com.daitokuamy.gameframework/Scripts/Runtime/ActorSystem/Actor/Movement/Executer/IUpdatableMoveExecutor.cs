namespace GameFramework.ActorSystem {
    /// <summary>
    /// 実行中リクエストの更新に対応する実行用インターフェース
    /// </summary>
    public interface IUpdatableMoveExecutor<TRequest> : IMoveExecutor<TRequest> 
        where TRequest : struct, IMoveRequest {
        /// <summary>
        /// 実行中の移動リクエスト内容を更新する
        /// </summary>
        /// <param name="request">更新後の移動リクエスト</param>
        void Update(in TRequest request);
    }
}
