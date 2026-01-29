namespace GameFramework.ActorSystems {
    /// <summary>
    /// 実行器パイプラインの内部インターフェース
    /// </summary>
    internal interface IMoveExecutorPipeline {
        /// <summary>現在実行中かどうか</summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// 実行中の処理を進行する
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        RunResult Tick(float deltaTime);
        
        /// <summary>
        /// 実行中の処理をキャンセルする
        /// </summary>
        void Cancel();
    }
}