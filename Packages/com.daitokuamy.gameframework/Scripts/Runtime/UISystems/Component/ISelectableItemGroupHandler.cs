namespace GameFramework.UISystems {
    /// <summary>
    /// SelectableItemGroupと連携するためのハンドラーインターフェース
    /// </summary>
    public interface ISelectableItemGroupHandler {
        /// <summary>
        /// 選択肢変化通知
        /// </summary>
        void OnChangedCurrentItem(int index, ISelectableItem item);
    }
}