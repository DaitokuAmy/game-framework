using System;
using System.Threading;

namespace GameFramework.Core {
    /// <summary>
    /// スコープ管理用インターフェース
    /// </summary>
    public interface IScope {
        /// <summary>スコープ終了通知</summary>
        event Action ExpiredEvent;
        
        /// <summary>キャンセル用Token</summary>
        CancellationToken Token { get; }
    }
}