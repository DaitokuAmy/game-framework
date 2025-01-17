using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.TaskSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// SituationService
    /// </summary>
    public partial class SituationService : IFixedUpdatableTask, ILateUpdatableTask, ITaskEventHandler, IDisposable {
        private readonly DisposableScope _scope;
        private readonly SituationContainer _situationContainer;
        private readonly SituationFlow _situationFlow;

        private TaskRunner _taskRunner;

        // アクティブか
        bool ITask.IsActive => true;

        private readonly Dictionary<Type, Situation> _situations;

        /// <summary>
        /// 現在のNodeが対象としているSituationを取得する
        /// </summary>
        public Situation CurrentNodeSituation => _situationFlow.CurrentNode?.Situation;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationService() {
            _scope = new DisposableScope();
            _situationContainer = new SituationContainer().ScopeTo(_scope);
            _situationFlow = new SituationFlow().ScopeTo(_scope);
            _situations = new Dictionary<Type, Situation>();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_taskRunner != null) {
                _taskRunner.Unregister(this);
                _taskRunner = null;
            }

            _situations.Clear();
            _scope.Dispose();
        }

        /// <summary>
        /// タスク更新
        /// </summary>
        void ITask.Update() {
            _situationContainer.Update();
            _situationFlow.Update();
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
                Debug.LogError("SituationContainer or SituationFlow is null");
                return;
            }

            SetupSituations(_scope);
            SetupFlow(_scope);
            SetupDebug(_scope);
        }

        /// <summary>
        /// 指定したSituationへ遷移する
        /// </summary>
        public IProcess Transition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return _situationFlow.Transition(type, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 指定したSituationへ遷移する
        /// </summary>
        public IProcess Transition<T>(Action<T> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) where T : Situation {
            return _situationFlow.Transition<T>(x => {
                if (x is T situation) {
                    onSetup?.Invoke(situation);
                }
            }, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移
        /// </summary>
        public IProcess Back(ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return _situationFlow.Back(overrideTransition, effects);
        }

        /// <summary>
        /// 現在のSituationNodeをリセット
        /// </summary>
        public IProcess Reset(Action<Situation> onSetup, params ITransitionEffect[] effects) {
            return _situationFlow.Reset(onSetup, effects);
        }

        /// <summary>
        /// 現在のSituationNodeをリセット
        /// </summary>
        public IProcess Reset(params ITransitionEffect[] effects) {
            return _situationFlow.Reset(null, effects);
        }

        /// <summary>
        /// Situationを登録する
        /// </summary>
        private void RegisterSituation(Situation situation) {
            if (situation == null) {
                return;
            }

            var type = situation.GetType();
            if (_situations.ContainsKey(type)) {
                Debug.LogWarning($"Already registered Situation type:{type}");
                return;
            }
            
            _situations[type] = situation;
        }

        /// <summary>
        /// 登録したSituationを取得する
        /// </summary>
        private Situation GetSituation<T>() where T : Situation {
            var type = typeof(T);
            return _situations.GetValueOrDefault(type);
        }
    }
}