namespace GameFramework.UISystems {
    /// <summary>
    /// 選択可能な項目を表すためのインターフェース
    /// </summary>
    public interface ISelectableItem {
        /// <summary>
        /// 選択された時の通知
        /// </summary>
        void OnSelectedItem();

        /// <summary>
        /// 選択解除された時の通知
        /// </summary>
        void OnDeselectedItem();

        /// <summary>
        /// 無効状態の設定
        /// </summary>
        void SetDisabled(bool disabled);
    }
}