namespace GameFramework.CommandSystems {
    /// <summary>
    /// コマンド用インターフェース
    /// </summary>
    public interface ICommand {
        /// <summary>優先順位(0以上, 高いほうが優先度高)</summary>
        int Priority { get; }
        /// <summary>割り込みするか(自分より優先度の低い物が実行中だった場合、強制的に停止して実行する)</summary>
        bool Interrupt { get; }
        /// <summary>現在のステート</summary>
        CommandState CurrentState { get; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <returns>trueを返すと実行継続</returns>
        bool Start();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <returns>trueを返すと継続</returns>
        bool Update();

        /// <summary>
        /// 終了処理
        /// </summary>
        void Finish();

        /// <summary>
        /// 廃棄処理
        /// </summary>
        void Destroy();
    }
}