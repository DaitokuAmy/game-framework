namespace GameFramework.TaskSystems {
    /// <summary>
    /// LateUpdateに対応したMonoBehaviour
    /// </summary>
    public abstract class FixedUpdatableTaskBehaviour : LateUpdatableTaskBehaviour, IFixedUpdatableTask {
        /// <summary>
        /// 固定更新処理
        /// </summary>
        void IFixedUpdatableTask.FixedUpdate() {
            FixedUpdateInternal();
        }

        /// <summary>
        /// 固定更新処理(override用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
        }
    }
}