using GameFramework.TaskSystems;

namespace GameFramework.LogicSystems {
    /// <summary>
    /// 固定更新付きロジック処理
    /// </summary>
    public abstract class FixedLogic : Logic, IFixedUpdatableTask {
        /// <summary>
        /// タスク固定更新処理
        /// </summary>
        void IFixedUpdatableTask.FixedUpdate() {
            FixedUpdateInternal();
        }

        /// <summary>
        /// 更新処理(Override用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
        }
    }
}