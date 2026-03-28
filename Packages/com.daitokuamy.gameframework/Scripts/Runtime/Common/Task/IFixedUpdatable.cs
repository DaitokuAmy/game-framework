namespace GameFramework {
    /// <summary>
    /// FixedUpdateで呼び出されるUpdatableインターフェース
    /// </summary>
    public interface IFixedUpdatable : IUpdatableBase {
        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();
    }
}
