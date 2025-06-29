using System;

namespace GameFramework.Core {
    /// <summary>
    /// 完了通知付き、一連の処理を表すインターフェース
    /// </summary>
    public interface IEventProcess : IProcess {
        /// <summary>終了通知</summary>
        event Action OnExitEvent;
    }
    
    /// <summary>
    /// 完了通知付き、一連の処理を表すインターフェース
    /// </summary>
    public interface IEventProcess<T> : IProcess<T> {
        /// <summary>終了通知</summary>
        event Action<T> OnExitEvent;
    }
}