namespace GameFramework {
    /// <summary>
    /// LateUpdateで呼び出されるUpdatableインターフェース
    /// </summary>
    public interface ILateUpdatable : IUpdatableBase {
        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();
    }
}
