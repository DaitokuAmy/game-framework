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

    /// <summary>
    /// 明示的に戻り値を与えるタイプのDialog用インターフェース
    /// </summary>
    public interface IDialog<out T> : IDialog {
        /// <summary>
        /// 結果の取得
        /// </summary>
        /// <param name="selectedIndex">選択したIndex</param>
        T GetResult(int selectedIndex);
    }
}
