namespace GameFramework {
    /// <summary>
    /// Updateで呼び出されるUpdatableインターフェース
    /// </summary>
    public interface IUpdatable : IUpdatableBase {
        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();
    }
}