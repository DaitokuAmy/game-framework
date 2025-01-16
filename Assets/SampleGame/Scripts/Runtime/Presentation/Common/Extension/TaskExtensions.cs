using GameFramework.Core;
using GameFramework.TaskSystems;

namespace SampleGame.Presentation {
    /// <summary>
    /// Task関連の拡張メソッド
    /// </summary>
    public static class TaskExtensions {
        /// <summary>
        /// タスクへ登録
        /// </summary>
        public static void RegisterTask(this ITask source, TaskOrder order) {
            var taskRunner = Services.Get<TaskRunner>();
            if (source == null || taskRunner == null) return;
            taskRunner.Register(source, order);
        }

        /// <summary>
        /// タスクから登録除外
        /// </summary>
        public static void UnregisterTask(this ITask source) {
            var taskRunner = Services.Get<TaskRunner>();
            if (source == null || taskRunner == null) return;
            taskRunner.Unregister(source);
        }
    }
}