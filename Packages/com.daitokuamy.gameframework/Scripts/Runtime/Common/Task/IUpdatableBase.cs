namespace GameFramework {
    /// <summary>
    /// Updateで呼び出されるUpdatableインターフェース
    /// </summary>
    public interface IUpdatableBase {
        /// <summary>更新の有効状態</summary>
        bool IsActive { get; }
    }
}