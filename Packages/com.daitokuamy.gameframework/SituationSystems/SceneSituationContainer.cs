using GameFramework.TaskSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シーン遷移を行えるシチュエーションコンテナ
    /// </summary>
    public class SceneSituationContainer : SituationContainer, ILateUpdatableTask, ITaskEventHandler {
        private TaskRunner _taskRunner;

        // アクティブか
        bool ITask.IsActive => true;

        /// <summary>
        /// タスク更新
        /// </summary>
        void ITask.Update() {
            // Rootシーン更新
            Update();
        }

        /// <summary>
        /// タスク後更新
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            // Rootシーン更新
            LateUpdate();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            if (_taskRunner != null) {
                _taskRunner.Unregister(this);
                _taskRunner = null;
            }
        }

        /// <summary>
        /// 遷移用のTransition取得
        /// </summary>
        protected override ITransition GetDefaultTransition() {
            return new OutInTransition();
        }

        /// <summary>
        /// 遷移を行えるか
        /// </summary>
        protected override bool CheckTransitionInternal(Situation next, ITransition transition) {
            return next is SceneSituation && transition is OutInTransition;
        }

        /// <summary>
        /// TaskRunnerに登録された時の処理
        /// </summary>
        void ITaskEventHandler.OnRegistered(TaskRunner runner) {
            _taskRunner = runner;
        }

        /// <summary>
        /// TaskRunnerから登録を外された時の処理
        /// </summary>
        void ITaskEventHandler.OnUnregistered(TaskRunner runner) {
            if (runner == _taskRunner) {
                _taskRunner = null;
            }
        }
    }
}