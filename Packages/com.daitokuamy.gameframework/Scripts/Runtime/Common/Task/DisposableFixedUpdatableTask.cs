namespace GameFramework {
    /// <summary>
    /// 固定更新付きタスク用インターフェース
    /// </summary>
    public abstract class DisposableFixedUpdatableTask : DisposableLateUpdatableTask, IFixedUpdatableTask {
        /// <summary>
        /// 固定更新処理
        /// </summary>
        void IFixedUpdatableTask.FixedUpdate() {
            FixedUpdateInternal();
        }

        /// <summary>
        /// 更新処理(override用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
        }
    }
}