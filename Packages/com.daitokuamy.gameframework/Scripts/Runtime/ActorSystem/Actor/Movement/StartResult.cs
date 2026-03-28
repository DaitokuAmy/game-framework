namespace GameFramework.ActorSystem {
    /// <summary>
    /// モジュール開始結果
    /// </summary>
    public enum StartResult : byte {
        /// <summary>開始成功</summary>
        Accepted,
        /// <summary>開始失敗</summary>
        Rejected,
    }
}
