using GameFramework;
using GameFramework.Core;

namespace ThirdPersonEngine {
    /// <summary>
    /// Task関連のUtility
    /// </summary>
    public static class TaskUtility {
        /// <summary>利用するTaskRunner</summary>
        private static TaskRunner TaskRunner { get; set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public static void Initialize(TaskRunner taskRunner) {
            TaskRunner = taskRunner;
        }
        
        /// <summary>
        /// タスクへ登録
        /// </summary>
        public static void RegisterTask(this ITask source, TaskOrder order) {
            if (source == null || TaskRunner == null) {
                return;
            }

            TaskRunner.Register(source, order);
        }

        /// <summary>
        /// タスクから登録除外
        /// </summary>
        public static void UnregisterTask(this ITask source) {
            if (source == null || TaskRunner == null) {
                return;
            }

            TaskRunner.Unregister(source);
        }
    }
}