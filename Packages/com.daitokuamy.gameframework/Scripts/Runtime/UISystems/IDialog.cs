using System;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIDialog用のインターフェース
    /// </summary>
    public interface IDialog {
        /// <summary>選択Index通知用イベント</summary>
        event Action<int> SelectedIndexEvent;

        /// <summary>
        /// キャンセル処理
        /// </summary>
        void Cancel();
    }
}
