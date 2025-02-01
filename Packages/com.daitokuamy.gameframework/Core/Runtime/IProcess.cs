using System;
using System.Collections;

namespace GameFramework.Core {
    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public interface IProcess : IEnumerator {
        /// <summary>完了しているか</summary>
        bool IsDone { get; }
        /// <summary>エラー内容</summary>
        Exception Exception { get; }
    }

    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public interface IProcess<T> : IEnumerator {
        /// <summary>結果</summary>
        T Result { get; }
        /// <summary>完了しているか</summary>
        bool IsDone { get; }
        /// <summary>エラー内容</summary>
        Exception Exception { get; }
    }
    
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