using System;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIDialogクラス（UIDialogContainerに登録して使う想定の物）
    /// </summary>
    public class UIDialog : UIScreen, IDialog {
        /// <summary>キャンセルされた際に返すIndex</summary>
        protected virtual int CanceledIndex => -1;
        
        /// <inheritdoc/>
        public event Action<int> SelectedIndexEvent;

        /// <inheritdoc/>
        public void Cancel() {
            SelectIndex(CanceledIndex);
        }

        /// <summary>
        /// 選択処理
        /// </summary>
        public void SelectIndex(int index) {
            SelectedIndexEvent?.Invoke(index);
        }
    }
}
