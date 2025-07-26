namespace GameFramework {
    /// <summary>
    /// 遷移向き
    /// </summary>
    public enum TransitionDirection {
        None = -1,
        Forward,
        Back,
    }
    
    /// <summary>
    /// 遷移状態
    /// </summary>
    public enum TransitionState {
        /// <summary>無効値</summary>
        Invalid = -1,
        /// <summary>遷移待機状態</summary>
        Standby,
        /// <summary>初期化中</summary>
        Initializing,
        /// <summary>オープン中</summary>
        Opening,
        /// <summary>遷移完了</summary>
        Completed,
        /// <summary>遷移キャンセル</summary>
        Canceled,
    }
    
    /// <summary>
    /// 遷移ステップ
    /// </summary>
    public enum TransitionStep {
        /// <summary>読み込みまで</summary>
        Load,
        /// <summary>初期化まで</summary>
        Setup,
        /// <summary>完了まで</summary>
        Complete,
    }
}