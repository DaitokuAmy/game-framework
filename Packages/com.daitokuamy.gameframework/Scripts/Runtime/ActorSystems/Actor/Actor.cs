using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// アクター基底
    /// </summary>
    public abstract class Actor : ILateUpdatableTask, ITaskEventHandler, IDisposable {
        private readonly Dictionary<Type, ActorComponent> _actorComponentDict = new(32);
        private readonly SortedList<int, ActorComponent> _actorComponents = new(32);

        private TaskRunner _taskRunner;
        private bool _disposed;
        private DisposableScope _activeScope;

        /// <summary>Taskが有効状態か</summary>
        bool ITask.IsActive => IsActive;

        /// <summary>アクティブ状態</summary>
        public virtual bool IsActive => _activeScope != null;

        /// <summary>制御するBody</summary>
        public Body Body { get; }
        /// <summary>Transform情報</summary>
        public Transform Transform => Body.Transform;
        /// <summary>グローバル位置</summary>
        public Vector3 Position {
            get => Body.Position;
            set => Body.Position = value;
        }
        /// <summary>グローバル向き</summary>
        public Quaternion Rotation {
            get => Body.Rotation;
            set => Body.Rotation = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Actor(Body body) {
            Body = body;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            Deactivate();
            DisposeInternal();
            DisposeComponents();

            if (_taskRunner != null) {
                _taskRunner.Unregister(this);
                _taskRunner = null;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ITask.Update() {
            if (_activeScope != null) {
                var deltaTime = Body.LayeredTime.DeltaTime;
                for (var i = 0; i < _actorComponents.Count; i++) {
                    var component = _actorComponents[i];
                    component.Update(deltaTime);
                }

                UpdateInternal();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        void ILateUpdatableTask.LateUpdate() {
            if (_activeScope != null) {
                var deltaTime = Body.LayeredTime.DeltaTime;
                for (var i = 0; i < _actorComponents.Count; i++) {
                    var component = _actorComponents[i];
                    component.LateUpdate(deltaTime);
                }

                LateUpdateInternal();
            }
        }

        /// <summary>
        /// Taskの登録通知
        /// </summary>
        void ITaskEventHandler.OnRegistered(TaskRunner taskRunner) {
            _taskRunner = taskRunner;
        }

        /// <summary>
        /// Taskの登録解除通知
        /// </summary>
        void ITaskEventHandler.OnUnregistered(TaskRunner taskRunner) {
            if (taskRunner == _taskRunner) {
                _taskRunner = null;
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void UpdateInternal() {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected virtual void ActivateInternal(IScope scope) {
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected virtual void DeactivateInternal() {
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        public void Activate() {
            if (_disposed || _activeScope != null) {
                return;
            }

            _activeScope = new DisposableScope();
            ActivateInternal(_activeScope);
        }

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        public void Deactivate() {
            if (_activeScope == null) {
                return;
            }

            _activeScope.Dispose();
            _activeScope = null;
        }

        /// <summary>
        /// Componentの取得
        /// </summary>
        public TComponent GetComponent<TComponent>()
            where TComponent : ActorComponent {
            if (!_actorComponentDict.TryGetValue(typeof(TComponent), out var component)) {
                return null;
            }

            return (TComponent)component;
        }

        /// <summary>
        /// Componentの追加
        /// </summary>
        public TComponent AddComponent<TComponent>(TComponent component)
            where TComponent : ActorComponent {
            var type = typeof(TComponent);
            if (!_actorComponentDict.TryAdd(type, component)) {
                throw new InvalidOperationException($"Actor component {type} has already been added.");
            }

            component.Initialize();
            _actorComponents.Add(component.ExecutionOrder, component);
            return component;
        }

        /// <summary>
        /// Componentの廃棄
        /// </summary>
        private void DisposeComponents() {
            foreach (var pair in _actorComponents) {
                pair.Value.Dispose();
            }

            _actorComponents.Clear();
        }
    }
}