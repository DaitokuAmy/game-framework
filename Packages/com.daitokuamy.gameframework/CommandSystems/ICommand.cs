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
        /// <summary>追加時に自身より優先度の低いコマンドをキャンセルするか</summary>
        bool AddedCancelLowPriorityOthers { get; }
        /// <summary>実行時に自身より優先度の低いコマンドをキャンセルするか</summary>
        bool ExecutedCancelLowPriorityOthers { get; }
        /// <summary>現在のステート</summary>
        CommandState CurrentState { get; }
        /// <summary>キャンセル終了した場合の例外</summary>
        Exception Exception { get; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();

        /// <summary>
        /// 待機中更新処理
        /// </summary>
        /// <returns>trueを返すと継続</returns>
        bool StandbyUpdate();

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