namespace GameFramework.ActorSystems {
    /// <summary>
    /// 移動実行の共通インターフェース
    /// </summary>
    public interface IMoveExecutor<TRequest>
        where TRequest : struct, IMoveRequest {
        /// <summary>フォールバック時の優先度（大きいほど優先）</summary>
        int Priority { get; }

        /// <summary>
        /// 指定された移動リクエストによる移動開始を試行する
        /// </summary>
        /// <param name="request">移動リクエスト</param>
        /// <returns>開始結果</returns>
        StartResult TryStart(in TRequest request);

        /// <summary>
        /// 移動処理を進行する
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        /// <returns>実行結果</returns>
        RunResult Tick(float deltaTime);

        /// <summary>
        /// 実行中の移動を強制的にキャンセルする
        /// </summary>
        void Cancel();
    }
}