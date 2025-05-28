namespace GameFramework.CommandSystems {
    /// <summary>
    /// コマンドの状態
    /// </summary>
    public enum CommandState {
        /// <summary>無効状態</summary>
        Invalid = -1,
        /// <summary>初期化～開始</summary>
        Standby,
        /// <summary>開始～終了</summary>
        Executing,
        /// <summary>終了～廃棄</summary>
        Finished,
        /// <summary>廃棄済み</summary>
        Destroyed,
    }
}