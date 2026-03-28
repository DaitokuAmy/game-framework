namespace GameFramework.ActorSystem {
    /// <summary>
    /// モジュール実行結果
    /// </summary>
    public enum RunResult : byte {
        /// <summary>実行継続中</summary>
        Running,
        /// <summary>正常終了</summary>
        Succeeded,
        /// <summary>回復可能な失敗（フォールバック可能）</summary>
        FailedRecoverable,
        /// <summary>致命的な失敗（フォールバック不可）</summary>
        FailedHard,
        /// <summary>外部からキャンセルされた</summary>
        Cancelled,
    }
}
