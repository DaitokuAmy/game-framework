using System;
using GameFramework.CoroutineSystems;
using GameFramework.TaskSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション実行用のランナー
    /// </summary>
    public class SituationRunner : ISituationContainerProvider, IFixedUpdatableTask, ILateUpdatableTask, ITaskEventHandler, IDisposable {
        private TaskRunner _taskRunner;
        private SituationContainer _rootContainer;
        private CoroutineRunner _rootCoroutineRunner;

        // アクティブか
        bool ITask.IsActive => true;

        /// <summary>インターフェース用の子コンテナ返却プロパティ</summary>
        SituationContainer ISituationContainerProvider.Container => _rootContainer;

        /// <summary>遷移用のコンテナ</summary>
        public SituationContainer Container => _rootContainer;

        /// <summary>
        /// シチュエーション実行用クラス
        /// </summary>
        /// <param name="useStack">Stack機能を使うか</param>
        public SituationRunner(bool useStack = false) {
            _rootCoroutineRunner = new CoroutineRunner();
            _rootContainer = new SituationContainer(null, _rootCoroutineRunner, useStack);
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

        /// <summary>
        /// タスク更新
        /// </summary>
        public void Update() {
            _rootCoroutineRunner.Update();
            _rootContainer.Update();
        }

        /// <summary>
        /// タスク後更新
        /// </summary>
        public void LateUpdate() {
            _rootContainer.LateUpdate();
        }

        /// <summary>
        /// 物理更新
        /// </summary>
        public void FixedUpdate() {
            _rootContainer.FixedUpdate();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_rootCoroutineRunner != null) {
                _rootCoroutineRunner.Dispose();
                _rootCoroutineRunner = null;
            }
            
            if (_rootContainer != null) {
                _rootContainer.Dispose();
                _rootContainer = null;
            }
            
            if (_taskRunner != null) {
                _taskRunner.Unregister(this);
                _taskRunner = null;
            }
        }
    }
}