using System;
using System.Threading;

namespace GameFramework.Core {
    /// <summary>
    /// スコープ管理用インターフェース
    /// </summary>
    public interface IScope {
        // スコープ終了通知
        event Action OnExpired;
        
        /// <summary>キャンセル用Token</summary>
        CancellationToken Token { get; }
    }
}