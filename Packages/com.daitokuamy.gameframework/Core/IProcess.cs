using System;
using System.Collections;

namespace GameFramework.Core {
    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public interface IProcess : IEnumerator {
        // 完了しているか
        bool IsDone { get; }
        // エラー内容
        Exception Exception { get; }
    }

    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public interface IProcess<T> : IEnumerator {
        // 結果
        T Result { get; }
        // 完了しているか
        bool IsDone { get; }
        // エラー内容
        Exception Exception { get; }
    }
}