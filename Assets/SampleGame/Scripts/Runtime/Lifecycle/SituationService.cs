using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// SituationService
    /// </summary>
    public partial class SituationService : IFixedUpdatableTask, ILateUpdatableTask, ITaskEventHandler, IDisposable {
        private readonly DisposableScope _scope;
        private readonly SituationContainer _situationContainer;
        private readonly ISituationFlow _situationFlow;

        private TaskRunner _taskRunner;

        /// <summary>アクティブか</summary>
        bool ITask.IsActive => true;

        /// <summary>現在のSituation</summary>
        public Situation CurrentSituation => _situationFlow.Current;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationService() {
            _scope = new DisposableScope();
            _situationContainer = new SituationContainer("Main").RegisterTo(_scope);
            _situationFlow = new SituationTree(_situationContainer, "Main").RegisterTo(_scope);
            //_situationFlow = new SituationStack(_situationContainer).RegisterTo(_scope);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_taskRunner != null) {
                _taskRunner.Unregister(this);
                _taskRunner = null;
            }

            _scope.Dispose();
        }

        /// <summary>
        /// タスク更新
        /// </summary>
        void ITask.Update() {
            _situationContainer.Update();
        }

        /// <summary>
        /// タスク後更新
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            _situationContainer.LateUpdate();
        }

        /// <summary>
        /// 物理更新
        /// </summary>
        void IFixedUpdatableTask.FixedUpdate() {
            _situationContainer.FixedUpdate();
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
        /// 初期化処理
        /// </summary>
        public void Initialize() {
            if (_scope.Disposed) {
                Debug.LogError("SituationService is disposed");
                return;
            }

            if (_situationContainer == null || _situationFlow == null) {
                Debug.LogError("SituationContainer or SituationTree is null");
                return;
            }

            SetupContainer(_scope);
            SetupTree(_situationFlow as SituationTree, _scope);
            SetupDebug(_scope);
        }

        /// <summary>
        /// 指定したSituationへ遷移する
        /// </summary>
        public TransitionHandle Transition(Type type, Action<Situation> onSetup = null, TransitionType transitionType = TransitionType.ScreenDefault) {
            var (transition, effects) = GetTransitionInfo(transitionType);
            return _situationFlow.Transition(type, onSetup, transition, effects);
        }

        /// <summary>
        /// 指定したSituationへ遷移する
        /// </summary>
        public TransitionHandle Transition<T>(Action<T> onSetup = null, TransitionType transitionType = TransitionType.ScreenDefault) where T : Situation {
            var (transition, effects) = GetTransitionInfo(transitionType);
            return _situationFlow.Transition(onSetup, transition, effects);
        }

        /// <summary>
        /// 戻り遷移
        /// </summary>
        public TransitionHandle Back(Action<Situation> onSetup = null, TransitionType transitionType = TransitionType.ScreenDefault) {
            var (transition, effects) = GetTransitionInfo(transitionType);
            return _situationFlow.Back(onSetup, transition, effects);
        }

        /// <summary>
        /// 現在のSituationNodeをリセット
        /// </summary>
        public TransitionHandle Reset(Action<Situation> onSetup = null) {
            var (_, effects) = GetTransitionInfo(TransitionType.SceneDefault);
            return _situationFlow.Reset(onSetup, effects);
        }
    }
}