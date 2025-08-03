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
    public interface IProcess<T> : IProcess {
        /// <summary>結果</summary>
        T Result { get; }
    }
}