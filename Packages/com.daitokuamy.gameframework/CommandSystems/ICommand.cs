using System;

namespace GameFramework.CommandSystems {
    /// <summary>
    /// コマンド用インターフェース
    /// </summary>
    public interface ICommand {
        /// <summary>優先順位(0以上, 高いほうが優先度高)</summary>
        int Priority { get; }
        /// <summary>スタンバイ中の他Commandの実行をBlockするか</summary>
        bool BlockStandbyOthers { get; }
        /// <summary>実行中のCommandが無くなるまでスタンバイし続けるか</summary>
        bool WaitExecutionOthers { get; }
        /// <summary>現在のステート</summary>
        CommandState CurrentState { get; }
        /// <summary>キャンセル終了した場合の例外</summary>
        Exception Exception { get; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="manager">Command実行を行うManager</param>
        void Initialize(CommandManager manager);

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <returns>trueを返すと実行開始</returns>
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